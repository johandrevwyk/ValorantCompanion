using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.Network.CurrentGameEndpoints.DataTypes;
using RadiantConnect.Network.PVPEndpoints.DataTypes;
using RadiantConnect.RConnect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ValorantCompanion
{
    public partial class MatchDetails : MaterialForm
    {
        private Initiator _initiator;

        private const int AgentImageSize = 48; // Size of agent images
        private const int BoxPadding = 3;      // Extra space above and below the image

        public MatchDetails()
        {
            InitializeComponent();

            _initiator = GlobalClient.Initiator;

            // Setup MaterialSkin
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

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;

            LoadPlayers();
        }

        private async void LoadPlayers()
        {
            try
            {
                // Prepare cache folder
                string cacheFolder = Path.Combine(Application.StartupPath, "cache");
                if (!Directory.Exists(cacheFolder))
                    Directory.CreateDirectory(cacheFolder);

                // Fetch current game info
                //var pregamePlayer = await _initiator.Endpoints.PreGameEndpoints.FetchPreGameMatchAsync(GlobalClient.UserId);
                var currentGamePlayer = await _initiator.Endpoints.CurrentGameEndpoints.GetCurrentGamePlayerAsync(GlobalClient.UserId);
                var matchId = currentGamePlayer.MatchId;

                // Get full match data
                //var pregame = await _initiator.Endpoints.PreGameEndpoints.FetchPreGameMatchAsync(pregameID);
                var globalGame = await _initiator.Endpoints.CurrentGameEndpoints.GetCurrentGameMatchAsync(matchId);
                var playersList = globalGame.Players.ToList();


                // Clear previous items
                flowPlayers.Controls.Clear();

                // Load competitive tiers JSON once
                var tierDoc = await LoadJsonWithCacheAsync(
                    "https://valorant-api.com/v1/competitivetiers/564d8e28-c226-3180-6285-e48a390db8b1",
                    cacheFolder
                );
                var tiersData = tierDoc.RootElement.GetProperty("data").GetProperty("tiers");

                foreach (var player in playersList)
                {
                    var playeruuid = player.Subject;
                    var playername = await RConnectMethods.GetRiotIdByPuuidAsync(_initiator, playeruuid);
                    var playerteam = player.TeamID;
                    var playeragent = player.CharacterID;

                    // Fetch the player's MMR data
                    PlayerMMR? playerMmr = await _initiator.Endpoints.PvpEndpoints.FetchPlayerMMRAsync(playeruuid);

                    // Fetch the current season ID
                    string? seasonId = await _initiator.FetchCurrentSeasonIdAsync();

                    // Default to 0 (Unranked)
                    long playertier = 0;

                    if (playerMmr != null &&
                        !string.IsNullOrEmpty(seasonId) &&
                        playerMmr.QueueSkills?.Competitive?.SeasonalInfoBySeasonID != null &&
                        playerMmr.QueueSkills.Competitive.SeasonalInfoBySeasonID.TryGetValue(seasonId, out var seasonData) &&
                        seasonData != null)
                    {
                        playertier = seasonData.CompetitiveTier ?? 0;
                    }

                    // Find matching rank icon
                    string tierIconUrl = null;
                    foreach (var tierEntry in tiersData.EnumerateArray())
                    {
                        int tierValue = tierEntry.GetProperty("tier").GetInt32();
                        if (tierValue == playertier)
                        {
                            tierIconUrl = tierEntry.GetProperty("largeIcon").GetString();
                            break;
                        }
                    }

                    // Background color based on team
                    var backgroundColor = playerteam == "Blue"
                        ? Color.FromArgb(20, 20, 40)
                        : Color.FromArgb(40, 20, 20);

                    // --- UI Layout ---
                    int panelHeight = AgentImageSize + (BoxPadding * 4); // Slightly bigger than agent
                    int panelWidth = AgentImageSize * 7; // More space for name and rank

                    // Player container panel
                    var playerPanel = new Panel
                    {
                        Width = panelWidth,
                        Height = panelHeight,
                        Margin = new Padding(5),
                        BackColor = backgroundColor
                    };

                    // Agent image
                    var agentImage = new PictureBox
                    {
                        Width = AgentImageSize,
                        Height = AgentImageSize,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point(5, (panelHeight - AgentImageSize) / 2),
                        BackColor = Color.Transparent
                    };

                    var agentImageUrl = GetAgentImageUrl(playeragent);
                    if (!string.IsNullOrEmpty(agentImageUrl))
                    {
                        try
                        {
                            using (var client = new System.Net.WebClient())
                            {
                                var data = await client.DownloadDataTaskAsync(agentImageUrl);
                                using (var ms = new System.IO.MemoryStream(data))
                                {
                                    agentImage.Image = Image.FromStream(ms);
                                }
                            }
                        }
                        catch
                        {
                            agentImage.BackColor = Color.Gray;
                        }
                    }

                    // Player name label
                    var nameLabel = new Label
                    {
                        Text = playername,
                        AutoSize = false,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Location = new Point(agentImage.Right + 10, (panelHeight - 20) / 2),
                        Width = panelWidth - AgentImageSize - 90, // Room for rank icon
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    // Rank icon (80% of agent icon size)
                    int rankSize = (int)(AgentImageSize * 0.8);
                    var rankImage = new PictureBox
                    {
                        Width = rankSize,
                        Height = rankSize,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point(panelWidth - rankSize - 10, (panelHeight - rankSize) / 2),
                        BackColor = Color.Transparent
                    };

                    if (!string.IsNullOrEmpty(tierIconUrl))
                    {
                        try
                        {
                            rankImage.Image = await LoadImageWithCacheAsync(tierIconUrl, cacheFolder);
                        }
                        catch
                        {
                            rankImage.BackColor = Color.Gray;
                        }
                    }

                    // Add controls
                    playerPanel.Controls.Add(agentImage);
                    playerPanel.Controls.Add(nameLabel);
                    playerPanel.Controls.Add(rankImage);

                    flowPlayers.Controls.Add(playerPanel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load current game: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private string GetAgentImageUrl(string agentUuid)
        {
            if (string.IsNullOrWhiteSpace(agentUuid))
                return null;

            // Replace this with your actual agent image logic
            return $"https://media.valorant-api.com/agents/{agentUuid}/displayicon.png";
        }
    }
}
