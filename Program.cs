using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JustGivingFetcher
{
    internal class Program
    {
        private static string AppId => ConfigurationManager.AppSettings.Get("JustGiving.AppId");

        private static string PageName => ConfigurationManager.AppSettings.Get("JustGiving.PageName");

        private static string OutputPath => ConfigurationManager.AppSettings.Get("OutputPath");

        private static int PollRateSeconds => int.Parse(ConfigurationManager.AppSettings.Get("PollRateSeconds"));

        public static async Task Main(string[] args)
        {
            // Ensure directory exists.
            EnsureDirectory(OutputPath);

            // Get poll rate.
            var pollRate = TimeSpan.FromSeconds(PollRateSeconds);

            while (true)
            {
                try
                {
                    Log("Starting poll.");

                    // Get data from API.
                    var details = await JustGivingClient.GetPageDetails(AppId, PageName);
                    var donations = await JustGivingClient.GetDonations(AppId, PageName);

                    Log("Retrieved data.");

                    // Get desired values.
                    var symbol = details.Value<string>("currencySymbol");
                    var target = details.Value<decimal>("fundraisingTarget");
                    var current = details.Value<decimal>("grandTotalRaisedExcludingGiftAid");

                    var donationObjs = donations.Value<JArray>("donations");
                    var latestDonation = donationObjs.FirstOrDefault();

                    var donationName = latestDonation?.Value<string>("donorDisplayName") ?? string.Empty;
                    var donationAmount = latestDonation?.Value<decimal>("donorLocalAmount") ?? 0;
                    var donationCurrency = latestDonation?.Value<string>("donorLocalCurrencyCode") ?? "";

                    var currentText = $"{symbol}{current:N2}";
                    var targetText = $"{symbol}{target:N2}";
                    var combinedText = $"{currentText} / {targetText}";
                    var donationText = $"{donationName} - {donationAmount:N2} {donationCurrency}";

                    // Update text files.
                    File.WriteAllText(Path.Combine(OutputPath, "current.txt"), currentText);
                    Log($"Updating current.txt: {currentText}");
                
                    File.WriteAllText(Path.Combine(OutputPath, "target.txt"), targetText);
                    Log($"Updating target.txt: {targetText}");
                
                    File.WriteAllText(Path.Combine(OutputPath, "combined.txt"), combinedText);
                    Log($"Updating combined.txt: {combinedText}");
                
                    File.WriteAllText(Path.Combine(OutputPath, "donation.txt"), donationText);
                    Log($"Updating donation.txt: {donationText}");

                    Log($"Sleeping for {pollRate:c}.");
                }
                catch (Exception e)
                {
                    // Log exception and continue.
                    Log(e.Message);
                }

                // Sleep until next poll time.
                Thread.Sleep(pollRate);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private static void EnsureDirectory(string path)
        {
            var directoryName = Path.GetDirectoryName(path);

            if (string.IsNullOrWhiteSpace(directoryName))
            {
                Console.WriteLine($"Could not parse directory from '{path}'");
                return;
            }

            Directory.CreateDirectory(directoryName);
        }

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:u}] {message}");
        }
    }
}