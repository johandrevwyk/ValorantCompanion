using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.Network.CurrentGameEndpoints.DataTypes;
using RadiantConnect.Network.PreGameEndpoints.DataTypes;
using RadiantConnect.Network.PVPEndpoints.DataTypes;
using RadiantConnect.RConnect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
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

            // Prevent resizing
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

                // --- STEP 1: Try to get CurrentGamePlayer ---
                CurrentGamePlayer currentGamePlayer = null;
                try
                {
                    currentGamePlayer = await _initiator.Endpoints.CurrentGameEndpoints.GetCurrentGamePlayerAsync(GlobalClient.UserId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CurrentGame fetch failed: {ex.Message}");
                }

                // --- STEP 2: Try to get PreGamePlayer ---
                PreGamePlayer pregamePlayer = null;
                try
                {
                    pregamePlayer = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync(GlobalClient.UserId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"PreGame fetch failed: {ex.Message}");
                }

                // --- STEP 3: Determine which data to use ---
                bool inCurrentGame = currentGamePlayer != null && !string.IsNullOrEmpty(currentGamePlayer.MatchId);
                bool inPreGame = pregamePlayer != null && !string.IsNullOrEmpty(pregamePlayer.MatchId);

                if (!inCurrentGame && !inPreGame)
                {
                    Console.WriteLine("User is not in a match or pregame.");
                    return; // Nothing to display
                }

                string matchId = inCurrentGame ? currentGamePlayer.MatchId : pregamePlayer.MatchId;

                // --- STEP 4: Fetch full match data ---
                List<dynamic> playersList = new List<dynamic>();

                try
                {
                    if (inCurrentGame)
                    {
                        var globalGame = await _initiator.Endpoints.CurrentGameEndpoints.GetCurrentGameMatchAsync(matchId);
                        playersList = globalGame.Players.ToList<dynamic>();
                    }
                    else
                    {
                        var globalPreGame = await _initiator.Endpoints.PreGameEndpoints.FetchPreGameMatchAsync(matchId);
                        playersList = globalPreGame.AllyTeam.Players.ToList<dynamic>();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to fetch match data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Console.WriteLine($"Loaded {playersList.Count} players.");

                // --- STEP 5: Clear old UI ---
                flowPlayers.Controls.Clear();

                // Load competitive tiers JSON once
                var tierDoc = await LoadJsonWithCacheAsync(
                    "https://valorant-api.com/v1/competitivetiers/564d8e28-c226-3180-6285-e48a390db8b1",
                    cacheFolder
                );
                var tiersData = tierDoc.RootElement.GetProperty("data").GetProperty("tiers");

                // --- STEP 6: Loop through players and build UI ---
                foreach (var player in playersList)
                {
                    var playeruuid = player.Subject;

                    // If it's PreGame, team data may not exist yet
                    string playerteam = null;
                    if (inCurrentGame && player.GetType().GetProperty("TeamID") != null)
                    {
                        playerteam = player.TeamID;
                    }

                    var playername = await RConnectMethods.GetRiotIdByPuuidAsync(_initiator, playeruuid);
                    var playeragent = player.CharacterID;
                    bool playerincognito = false;

                    // Safely check for PlayerIdentity when in pregame
                    if (player.GetType().GetProperty("PlayerIdentity") != null)
                    {
                        var identity = player.PlayerIdentity;
                        if (identity != null && identity.GetType().GetProperty("Incognito") != null)
                        {
                            playerincognito = identity.Incognito;
                        }
                    }

                    // --- Fetch MMR ---
                    PlayerMMR playerMmr = null;
                    try
                    {
                        playerMmr = await _initiator.Endpoints.PvpEndpoints.FetchPlayerMMRAsync(playeruuid);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to fetch MMR for {playername}: {ex.Message}");
                    }

                    // --- Get current season ID ---
                    string seasonId = await _initiator.FetchCurrentSeasonIdAsync();
                    long playertier = 0;

                    if (playerMmr != null &&
                        !string.IsNullOrEmpty(seasonId) &&
                        playerMmr.QueueSkills?.Competitive?.SeasonalInfoBySeasonID != null &&
                        playerMmr.QueueSkills.Competitive.SeasonalInfoBySeasonID.TryGetValue(seasonId, out var seasonData) &&
                        seasonData != null)
                    {
                        playertier = seasonData.CompetitiveTier ?? 0;
                    }

                    // --- Find rank icon URL ---
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

                    // --- Background color logic ---
                    Color backgroundColor = Color.FromArgb(25, 25, 25); // default neutral
                    if (inCurrentGame)
                    {
                        if (playerteam == "Blue")
                            backgroundColor = Color.FromArgb(20, 20, 40);
                        else if (playerteam == "Red")
                            backgroundColor = Color.FromArgb(40, 20, 20);
                    }

                    // --- Layout sizes ---
                    int panelHeight = AgentImageSize + (BoxPadding * 4);
                    int panelWidth = AgentImageSize * 7;

                    // --- Player container panel ---
                    var playerPanel = new Panel
                    {
                        Width = panelWidth,
                        Height = panelHeight,
                        Margin = new Padding(5),
                        BackColor = backgroundColor
                    };

                    // --- Agent Image ---
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
                                using (var ms = new MemoryStream(data))
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

                    // --- Player Name ---
                    var nameLabel = new Label
                    {
                        Text = playername,
                        AutoSize = false,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Location = new Point(agentImage.Right + 10, (panelHeight - 20) / 2),
                        Width = panelWidth - AgentImageSize - 100,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    // --- Incognito Icon ---
                    int incognitoSize = (int)(AgentImageSize * 0.7);
                    var incognitoIcon = new PictureBox
                    {
                        Width = incognitoSize,
                        Height = incognitoSize,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BackColor = Color.Transparent,
                        Visible = playerincognito,
                        Location = new Point(nameLabel.Right + 5, (panelHeight - incognitoSize) / 2)
                    };

                    if (playerincognito)
                    {
                        try
                        {
                            using (var client = new System.Net.WebClient())
                            {
                                var data = await client.DownloadDataTaskAsync(
                                    "https://cdn.discordapp.com/attachments/861327604199587860/1413549994832822272/image.png?ex=68bc5685&is=68bb0505&hm=d1bebfc5e58660e332ce514fc6225c34fb0f1f3adfcaa429ec4dc24fe1a4e8d9&"
                                );
                                using (var ms = new MemoryStream(data))
                                {
                                    incognitoIcon.Image = Image.FromStream(ms);
                                }
                            }
                        }
                        catch
                        {
                            incognitoIcon.BackColor = Color.Gray;
                        }
                    }

                    // --- Rank Icon ---
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

                    // --- Add controls to panel ---
                    playerPanel.Controls.Add(agentImage);
                    playerPanel.Controls.Add(nameLabel);
                    if (playerincognito)
                        playerPanel.Controls.Add(incognitoIcon);
                    playerPanel.Controls.Add(rankImage);

                    // --- Add to flow layout ---
                    flowPlayers.Controls.Add(playerPanel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error loading players: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        // --- Helper: Load JSON with Cache ---
        private async Task<JsonDocument> LoadJsonWithCacheAsync(string url, string cacheFolder)
        {
            string hash = GetMd5Hash(url);
            string cachePath = Path.Combine(cacheFolder, hash + ".json");

            if (File.Exists(cachePath))
            {
                string cachedJson = await File.ReadAllTextAsync(cachePath);
                return JsonDocument.Parse(cachedJson);
            }

            using var http = new HttpClient();
            string json = await http.GetStringAsync(url);
            await File.WriteAllTextAsync(cachePath, json);
            return JsonDocument.Parse(json);
        }

        // --- Helper: Load Image with Cache ---
        private async Task<Image> LoadImageWithCacheAsync(string url, string cacheFolder)
        {
            string hash = GetMd5Hash(url);
            string cachePath = Path.Combine(cacheFolder, hash + ".png");

            if (File.Exists(cachePath))
            {
                return Image.FromFile(cachePath);
            }

            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(cachePath, bytes);

            using var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        // --- Helper: MD5 Hash ---
        private string GetMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        // --- Helper: Agent Image URL ---
        private string GetAgentImageUrl(string agentUuid)
        {
            if (string.IsNullOrWhiteSpace(agentUuid))
                return null;

            return $"https://media.valorant-api.com/agents/{agentUuid}/displayicon.png";
        }
    }
}
