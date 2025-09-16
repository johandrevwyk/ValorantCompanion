using RadiantConnect;
using RadiantConnect.Authentication.DriverRiotAuth.Records;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static RadiantConnect.ValorantApi.CompetitiveTiers;

namespace ValorantCompanion
{
    public static class GlobalClient
    {
        public static Initiator Initiator { get; set; } = null!;
        public static string GlzUrl { get; set; } = string.Empty;
        public static string PdUrl { get; set; } = string.Empty;
        public static string SharedUrl { get; set; } = string.Empty;
        public static string UserId { get; set; } = string.Empty;
        public static string AuthMethod { get; set; } = string.Empty;
        public static string SeasonId { get; set; } = string.Empty;
        public static string EpisodeId { get; set; } = "03621f52-342b-cf4e-4f86-9350a49c6d04"; // fuck knows whats going on here
    }

    public static class ImageHelper
    {
        public static async Task<Image?> DownloadImageAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                using var client = new HttpClient();
                using var stream = await client.GetStreamAsync(url);
                return Image.FromStream(stream);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<Image?> GetAgentImageAsync(string playerAgent)
        {
            if (string.IsNullOrEmpty(playerAgent))
                return null;

            try
            {
                var agentData = await RadiantConnect.ValorantApi.Agents.GetAgentAsync(playerAgent);
                string? agentImageUrl = agentData?.Data?.DisplayIcon ?? agentData?.Data?.DisplayIconSmall;

                return await DownloadImageAsync(agentImageUrl);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<Image?> GetTierImageAsync(long playerTier, CompetitiveTiersData tiersData)
        {
            if (tiersData?.Data == null || playerTier <= 0 || string.IsNullOrEmpty(GlobalClient.EpisodeId))
                return null;

            try
            {
                var seasonDatum = tiersData.Data.FirstOrDefault(d => d.Uuid == GlobalClient.EpisodeId);
                if (seasonDatum?.Tiers == null)
                    return null;

                var matchingTier = seasonDatum.Tiers.FirstOrDefault(t => t.Tier == (int)playerTier);
                if (matchingTier == null || string.IsNullOrEmpty(matchingTier.LargeIcon))
                    return null;

                Console.WriteLine($"Image URL: {matchingTier.LargeIcon}");

                return await DownloadImageAsync(matchingTier.LargeIcon);
            }
            catch
            {
                return null;
            }
        }
    }

    public class Weapon
    {
        public string Uuid { get; set; }
        public string DisplayName { get; set; }
        public string DefaultSkinUuid { get; set; }
        public string DisplayIcon { get; set; }
        public List<Skin> Skins { get; set; }
    }

    public class Skin
    {
        public string Uuid { get; set; }
        public string DisplayName { get; set; }
        public string DisplayIcon { get; set; }
    }


    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [STAThread]
        static void Main()
        {

            // Allocate a console for debugging
            //AllocConsole();
            Console.WriteLine("Global debug console initialized.");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start with Auth form
            Application.Run(new Auth());
        }
    }
}
