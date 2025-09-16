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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RadiantConnect.ValorantApi.CompetitiveTiers;
using static RadiantConnect.ValorantApi.Gamemodes;
using static RadiantConnect.ValorantApi.Maps;

namespace ValorantCompanion
{
    public partial class MatchDetails : MaterialForm
    {
        private Initiator _initiator;

        private const int AgentImageSize = 48;
        private const int BoxPadding = 3;

        // Loading overlay
        private Panel loadingPanel;
        private MaterialProgressBar progressBar;
        private MaterialLabel loadingLabel;

        public MatchDetails()
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

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = this.MaximumSize = this.Size;

            InitializeLoadingPanel();
            LoadPlayers();
        }

        private void InitializeLoadingPanel()
        {
            loadingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(150, 0, 0, 0),
                Visible = false
            };

            progressBar = new MaterialProgressBar
            {
                Width = 250,
                Height = 6,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30
            };

            loadingLabel = new MaterialLabel
            {
                Text = "Loading...",
                ForeColor = Color.White,
                Font = new Font("Roboto", 11, FontStyle.Bold),
                AutoSize = true
            };

            loadingPanel.Controls.Add(loadingLabel);
            loadingPanel.Controls.Add(progressBar);
            this.Controls.Add(loadingPanel);
            loadingPanel.BringToFront();

            void CenterControls()
            {
                progressBar.Location = new Point(
                    (loadingPanel.ClientSize.Width - progressBar.Width) / 2,
                    (loadingPanel.ClientSize.Height - progressBar.Height) / 2
                );

                loadingLabel.Location = new Point(
                    (loadingPanel.ClientSize.Width - loadingLabel.Width) / 2,
                    progressBar.Top - loadingLabel.Height - 10
                );
            }

            CenterControls();
            this.Resize += (s, e) => CenterControls();
        }

        private async Task<string> GetPodLocationAsync(string podId)
        {
            if (string.IsNullOrWhiteSpace(podId))
                return podId;

            try
            {
                using var client = new HttpClient();
                string url = $"https://api.radiantconnect.ca/api/gamedata/pods/pod/{podId}";
                var response = await client.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("data", out var dataProp) &&
                        dataProp.TryGetProperty("pod_location", out var locProp))
                    {
                        return locProp.GetString() ?? podId;
                    }
                }
            }
            catch
            {
                // fallback to podId
            }

            return podId;
        }


        private async void LoadPlayers()
        {
            loadingPanel.Visible = true;
            try
            {
                string? map = null;
                string? server = null;
                string? mode = null;
                string? startingSide = null;

                string cacheFolder = Path.Combine(Application.StartupPath, "cache");
                if (!Directory.Exists(cacheFolder)) Directory.CreateDirectory(cacheFolder);

                CurrentGamePlayer currentGamePlayer = null;
                PreGamePlayer pregamePlayer = null;

                try { currentGamePlayer = await _initiator.Endpoints.CurrentGameEndpoints.GetCurrentGamePlayerAsync(); }
                catch { }
                try { pregamePlayer = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync(); }
                catch { }

                bool inCurrentGame = currentGamePlayer != null && !string.IsNullOrEmpty(currentGamePlayer.MatchId);
                bool inPreGame = pregamePlayer != null && !string.IsNullOrEmpty(pregamePlayer.MatchId);

                if (!inCurrentGame && !inPreGame)
                {
                    Console.WriteLine("User is not in a match or pregame.");
                    return;
                }

                string matchId = inCurrentGame ? currentGamePlayer!.MatchId : pregamePlayer!.MatchId;

                List<dynamic> playersList = new List<dynamic>();

                try
                {
                    if (inCurrentGame)
                    {
                        var globalGame = await _initiator.Endpoints.CurrentGameEndpoints.GetCurrentGameMatchAsync();
                        playersList = globalGame!.Players.ToList<dynamic>();
                        map = globalGame.MapID;
                        server = globalGame.GamePodID;
                        mode = globalGame.MatchmakingData?.QueueID ?? "custom"; // fallback to "custom"

                        if (!string.IsNullOrEmpty(mode))
                            mode = char.ToUpper(mode[0]) + mode.Substring(1);

                        lblServer.Text = await GetPodLocationAsync(server);
                        lblMode.Text = mode;
                        Console.WriteLine(mode);

                        var userPlayer = playersList.FirstOrDefault(p => p.Subject == GlobalClient.UserId);
                        if (userPlayer != null && userPlayer!.GetType().GetProperty("TeamID") != null)
                            startingSide = (userPlayer!.TeamID == "Red") ? "Attacker" : "Defender";
                    }
                    else
                    {
                        var globalPreGame = await _initiator.Endpoints.PreGameEndpoints.FetchPreGameMatchAsync();
                        playersList = globalPreGame!.AllyTeam.Players.ToList<dynamic>();
                        map = globalPreGame.MapID;
                        server = globalPreGame.GamePodID;
                        mode = globalPreGame.QueueID ?? "custom"; // fallback to "custom"

                        if (!string.IsNullOrEmpty(mode))
                            mode = char.ToUpper(mode[0]) + mode.Substring(1);

                        lblServer.Text = await GetPodLocationAsync(server);
                        lblMode.Text = mode;
                        Console.WriteLine(mode);

                        if (globalPreGame.AllyTeam != null)
                            startingSide = (globalPreGame.AllyTeam.TeamID == "Red") ? "Attacker" : "Defender";
                    }


                    if (!string.IsNullOrEmpty(map))
                    {
                        try
                        {
                            MapsData? mapsData = await GetMapsAsync();
                            if (mapsData?.Data == null) return;

                            var mapDatum = mapsData.Data.FirstOrDefault(m => m.MapUrl == map);
                            if (mapDatum == null) return;

                            lblMapName.Text = mapDatum.DisplayName;
                            lblStart.Text = $"Side: {startingSide}";

                            var splashUrl = mapDatum.Splash;
                            if (!string.IsNullOrEmpty(splashUrl))
                            {
                                using var client = new System.Net.Http.HttpClient();
                                using var stream = await client.GetStreamAsync(splashUrl);
                                imgMap.Image = Image.FromStream(stream);
                            }
                        }
                        catch
                        {
                            imgMap.Image = null;
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to fetch match data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                flowPlayers.Controls.Clear();

                CompetitiveTiersData? tiers = await GetCompetitiveTiersAsync();

                foreach (var player in playersList)
                {
                    await AddPlayerPanelAsync(player, inCurrentGame, inPreGame, startingSide, tiers, cacheFolder);
                }
            }
            finally
            {
                loadingPanel.Visible = false;
            }
        }

        private async Task AddPlayerPanelAsync(dynamic player, bool inCurrentGame, bool inPreGame, string startingSide, CompetitiveTiersData tiersData, string cacheFolder)
        {
            string playerUuid = player.Subject;
            string playerName = await RConnectMethods.GetRiotIdByPuuidAsync(_initiator, playerUuid);
            string playerAgent = player.CharacterID;

            // Incognito
            bool playerIncognito = false;
            if (player.GetType().GetProperty("PlayerIdentity") != null)
            {
                var identity = player.PlayerIdentity;
                if (identity != null && identity!.GetType().GetProperty("Incognito") != null)
                    playerIncognito = identity!.Incognito;
            }

            // --- MMR / Tier ---
            PlayerMMR playerMmr = null;
            try
            {
                playerMmr = await _initiator.Endpoints.PvpEndpoints.FetchPlayerMMRAsync(playerUuid);
            }
            catch
            {
                // ignore errors for now
            }

            // Get competitive tier from latest update, default to 0 if null
            long playerTier = 0;

            if (playerMmr?.LatestCompetitiveUpdate != null)
            {
                playerTier = playerMmr.LatestCompetitiveUpdate.TierAfterUpdate ?? 0;
            }

            var tierIcon = await ImageHelper.GetTierImageAsync(playerTier, tiersData);

            // Colors
            Color backgroundColor = Color.FromArgb(25, 25, 25);
            string effectiveTeam = inCurrentGame ? player.TeamID : (startingSide == "Attacker" ? "Red" : "Blue");
            if (effectiveTeam == "Blue") backgroundColor = Color.FromArgb(20, 20, 40);
            else if (effectiveTeam == "Red") backgroundColor = Color.FromArgb(40, 20, 20);

            // Panel
            int panelHeight = AgentImageSize + BoxPadding * 4;
            int panelWidth = 400; // wide enough to fit everything
            var playerPanel = new Panel
            {
                Width = panelWidth,
                Height = panelHeight,
                Margin = new Padding(5),
                BackColor = backgroundColor
            };

            // --- Agent image ---
            var agentImage = new PictureBox
            {
                Width = AgentImageSize,
                Height = AgentImageSize,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(5, (panelHeight - AgentImageSize) / 2),
                BackColor = Color.Transparent
            };

            if (!string.IsNullOrEmpty(playerAgent))
            {
                try
                {
                    Image? img = await ImageHelper.GetAgentImageAsync(playerAgent);
                    if (img != null)
                        agentImage.Image = img;
                    else
                        agentImage.BackColor = Color.Gray;
                }
                catch
                {
                    agentImage.BackColor = Color.Gray;
                }
            }

            // Rank icon
            int rankSize = 32;
            var rankImage = new PictureBox
            {
                Width = rankSize,
                Height = rankSize,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(panelWidth - rankSize - 10, (panelHeight - rankSize) / 2),
                BackColor = Color.Transparent
            };
            if (tierIcon != null)
            {
                try
                {
                    rankImage.Image = tierIcon;
                }
                catch { rankImage.BackColor = Color.Gray; }
            }

            // Incognito icon
            int incognitoSize = 24;
            var incognitoIcon = new PictureBox
            {
                Width = incognitoSize,
                Height = incognitoSize,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Visible = playerIncognito
            };
            if (playerIncognito)
            {
                incognitoIcon.Location = new Point(rankImage.Left - incognitoSize - 5, (panelHeight - incognitoSize) / 2);
                try
                {
                    using var client = new System.Net.WebClient();
                    var data = await client.DownloadDataTaskAsync("https://iili.io/KnSLXTB.png");
                    using var ms = new MemoryStream(data);
                    incognitoIcon.Image = Image.FromStream(ms);
                }
                catch { incognitoIcon.BackColor = Color.Gray; }
            }

            // Name label
            int nameWidth = playerIncognito ? incognitoIcon.Left - agentImage.Right - 10 : rankImage.Left - agentImage.Right - 10;
            var nameLabel = new Label
            {
                Text = playerName,
                AutoSize = false,
                Width = nameWidth,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(agentImage.Right + 10, (panelHeight - 20) / 2),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Add controls
            playerPanel.Controls.Add(agentImage);
            playerPanel.Controls.Add(nameLabel);
            if (playerIncognito) playerPanel.Controls.Add(incognitoIcon);
            playerPanel.Controls.Add(rankImage);

            flowPlayers.Controls.Add(playerPanel);
        }
    }
}
