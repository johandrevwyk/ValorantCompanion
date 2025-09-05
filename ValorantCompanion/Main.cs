using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.Network.CurrentGameEndpoints.DataTypes;
using RadiantConnect.Network.LocalEndpoints.DataTypes;
using RadiantConnect.Network.PreGameEndpoints.DataTypes;
using RadiantConnect.RConnect;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace ValorantCompanion
{
    public partial class Main : MaterialForm
    {
        private Initiator _initiator;

        public Main()
        {
            InitializeComponent();

            _initiator = GlobalClient.Initiator;

            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(this);
            skinManager.Theme = MaterialSkinManager.Themes.DARK;
            skinManager.ColorScheme = new ColorScheme(
                Primary.Indigo700,
                Primary.Indigo900,
                Primary.Indigo500,
                Accent.LightBlue200,
                TextShade.WHITE
            );

            this.Shown += Main_Shown;

            //Stop window from resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // disables resizing border
            this.MaximizeBox = false;                            // disable maximize
            this.MinimizeBox = false;                            // optional: set true if you want minimize
            this.SizeGripStyle = SizeGripStyle.Hide;             // hide size grip

            var fixedSize = this.Size;
            this.MinimumSize = fixedSize;
            this.MaximumSize = fixedSize;


            this.StartPosition = FormStartPosition.CenterScreen;

            Point fixedLocation = materialCard1.Location;
            Size fixedSize2 = materialCard1.Size;

            // Prevent moving/resizing
            materialCard1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            materialCard1.Dock = DockStyle.None;

            // Optional: enforce fixed location & size at runtime
            materialCard1.Location = fixedLocation;
            materialCard1.Size = fixedSize2;

            // Optional: handle runtime attempts to move/resize
            materialCard1.Resize += (s, e) => materialCard1.Size = fixedSize2;
            materialCard1.Move += (s, e) => materialCard1.Location = fixedLocation;
        }

        private void btnInstaLock_Click(object sender, EventArgs e)
        {
            var instaLock = new InstaLock();

            // Option 1: Non-blocking - user can still interact with Main form
            instaLock.Show();
        }

        /*private void brnMatch_Click(object sender, EventArgs e)
        {
            var match = new Match();

            match.Show();
        }*/

        private async void btnDodge_Click(object sender, EventArgs e)
        {
            try
            {
                var match = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync(GlobalClient.UserId);

                if (match?.MatchId == null)
                {
                    MessageBox.Show("You are not in a pre-game lobby.", "Cannot Dodge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var matchid = match.MatchId;
                await _initiator.Endpoints.PreGameEndpoints.QuitGameAsync(matchid);
                MessageBox.Show("You have left the pre-game lobby.", "Dodge Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (RadiantConnect.RadiantConnectNetworkStatusException ex)
            {
                // Check if it's a 404 / RESOURCE_NOT_FOUND, which means not in pre-game
                if (ex.Message.Contains("RESOURCE_NOT_FOUND"))
                {
                    MessageBox.Show("You are not in a pre-game lobby.", "Cannot Dodge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Failed to leave lobby: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to leave lobby: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void Main_Shown(object sender, EventArgs e)
        {
            try
            {
                // Prepare cache folder
                string cacheFolder = Path.Combine(Application.StartupPath, "cache");
                if (!Directory.Exists(cacheFolder))
                    Directory.CreateDirectory(cacheFolder);

                // Fetch player info, MMR, and loadout
                AliasInfo? alias = await _initiator.Endpoints.LocalEndpoints.GetAliasInfoAsync();
                var playermmr = await _initiator.Endpoints.PvpEndpoints.FetchPlayerMMRAsync(GlobalClient.UserId);
                var playerloadout = await _initiator.Endpoints.PvpEndpoints.FetchPlayerLoadoutAsync(GlobalClient.UserId);

                var playerCardUuid = playerloadout.Identity.PlayerCardID;

                if (alias != null)
                {
                    lblPlayerName.Text = $"{alias.GameName}#{alias.TagLine}";
                    lblRR.Text = $"{playermmr.LatestCompetitiveUpdate.RankedRatingAfterUpdate} / 100";

                    using var http = new HttpClient();

                    // --- Competitive Tier ---
                    var tierDoc = await LoadJsonWithCacheAsync("https://valorant-api.com/v1/competitivetiers/564d8e28-c226-3180-6285-e48a390db8b1", cacheFolder);

                    var tiersData = tierDoc.RootElement.GetProperty("data").GetProperty("tiers");

                    int playerTier = (int)playermmr.LatestCompetitiveUpdate.TierAfterUpdate;
                    string tierIconUrl = null;

                    foreach (var tierEntry in tiersData.EnumerateArray())
                    {
                        int tierValue = tierEntry.GetProperty("tier").GetInt32();
                        if (tierValue == playerTier)
                        {
                            tierIconUrl = tierEntry.GetProperty("largeIcon").GetString();
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(tierIconUrl))
                    {
                        imgRank.Image = await LoadImageWithCacheAsync(tierIconUrl, cacheFolder);
                    }

                    // --- Player Card ---
                    var cardDoc = await LoadJsonWithCacheAsync("https://valorant-api.com/v1/playercards", cacheFolder);
                    var cardsData = cardDoc.RootElement.GetProperty("data");

                    string cardUrl = null;

                    foreach (var card in cardsData.EnumerateArray())
                    {
                        string uuid = card.GetProperty("uuid").GetString();
                        if (uuid == playerCardUuid)
                        {
                            cardUrl = card.GetProperty("largeArt").GetString();
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(cardUrl))
                    {
                        imgCard.Image = await LoadImageWithCacheAsync(cardUrl, cacheFolder);
                    }

                    Console.WriteLine($"Player: {lblPlayerName.Text}");
                    Console.WriteLine($"RR: {lblRR.Text}");
                    Console.WriteLine($"Player Card URL: {cardUrl}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading player info: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<JsonDocument> LoadJsonWithCacheAsync(string url, string cacheFolder)
        {
            string hash = GetMd5Hash(url);
            string cachePath = Path.Combine(cacheFolder, hash + ".json");

            if (File.Exists(cachePath))
            {
                // Load JSON from cache
                string cachedJson = await File.ReadAllTextAsync(cachePath);
                return JsonDocument.Parse(cachedJson);
            }

            // Download JSON and cache it
            using var http = new HttpClient();
            string json = await http.GetStringAsync(url);
            await File.WriteAllTextAsync(cachePath, json);
            return JsonDocument.Parse(json);
        }

        private async Task<Image> LoadImageWithCacheAsync(string url, string cacheFolder)
        {
            using var http = new HttpClient();

            // Create a hashed filename for the URL
            string hash = GetMd5Hash(url);
            string cachePath = Path.Combine(cacheFolder, hash + ".png");

            // If image is cached, load it directly
            if (File.Exists(cachePath))
            {
                return Image.FromFile(cachePath);
            }

            // Otherwise, download and cache it
            var bytes = await http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(cachePath, bytes);

            using var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        private string GetMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }


        private async void GetUserInfo(object sender, EventArgs e)
        {


        }

        private async void btnMatch_Click(object sender, EventArgs e)
        {
            CurrentGamePlayer currentGamePlayer = null;
            PreGamePlayer pregamePlayer = null;

            // Try CurrentGame
            try
            {
                currentGamePlayer = await _initiator.Endpoints.CurrentGameEndpoints.GetCurrentGamePlayerAsync(GlobalClient.UserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CurrentGame fetch failed: {ex.Message}");
            }

            // Try PreGame
            try
            {
                pregamePlayer = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync(GlobalClient.UserId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PreGame fetch failed: {ex.Message}");
            }

            // Check if player is in *either* CurrentGame or PreGame
            bool inMatch = (currentGamePlayer != null && !string.IsNullOrEmpty(currentGamePlayer.MatchId))
                           || (pregamePlayer != null && !string.IsNullOrEmpty(pregamePlayer.MatchId));

            if (inMatch)
            {
                var match = new MatchDetails();
                match.Show();
            }
            else
            {
                MessageBox.Show("You are not in a match", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
