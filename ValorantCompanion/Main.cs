using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.Network;
using RadiantConnect.Network.CurrentGameEndpoints.DataTypes;
using RadiantConnect.Network.LocalEndpoints.DataTypes;
using RadiantConnect.Network.PreGameEndpoints.DataTypes;
using RadiantConnect.Network.PVPEndpoints.DataTypes;
using RadiantConnect.Network.StoreEndpoints.DataTypes;
using RadiantConnect.RConnect;
using RadiantConnect.SocketServices.InternalTcp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RadiantConnect.ValorantApi.CompetitiveTiers;
using static RadiantConnect.ValorantApi.PlayerCards;
using static RadiantConnect.ValorantApi.Seasons;
using static RadiantConnect.ValorantApi.Weapons;

namespace ValorantCompanion
{

    public partial class Main : MaterialForm
    {
        private Initiator _initiator;

        // Loading overlay
        private Panel loadingPanel;
        private MaterialProgressBar progressBar;
        private MaterialLabel loadingLabel;
        private SocketManager _socketManager;


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

            InitializeLoadingPanel();

            Shown += Main_Shown;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;

            var fixedSize = this.Size;
            this.MinimumSize = fixedSize;
            this.MaximumSize = fixedSize;
            this.StartPosition = FormStartPosition.CenterScreen;

            Point fixedLocation = materialCard1.Location;
            Size fixedSize2 = materialCard1.Size;
            materialCard1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            materialCard1.Dock = DockStyle.None;
            materialCard1.Location = fixedLocation;
            materialCard1.Size = fixedSize2;
            materialCard1.Resize += (s, e) => materialCard1.Size = fixedSize2;
            materialCard1.Move += (s, e) => materialCard1.Location = fixedLocation;

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
                loadingLabel.AutoSize = true;
                loadingLabel.PerformLayout();
                loadingLabel.Refresh();

                progressBar.Location = new Point(
                    (loadingPanel.ClientSize.Width - progressBar.Width) / 2,
                    (loadingPanel.ClientSize.Height - progressBar.Height) / 2
                );

                loadingLabel.Location = new Point(
                    (loadingPanel.ClientSize.Width - loadingLabel.Width) / 2,
                    progressBar.Top - loadingLabel.Height - 10
                );
            }

            loadingPanel.Layout += (s, e) => CenterControls();

            this.Resize += (s, e) => CenterControls();
        }

        private string? _lastLoopState = null;
        private DateTime _lastTriggerTime = DateTime.MinValue;
        private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(5);
        private MatchDetails? _matchDetailsForm = null;
        private Rectangle _lastMatchDetailsBounds = Rectangle.Empty;
        private InstaLock instaLockForm;
        private void RefreshMatchDetailsForm()
        {
            this.Invoke(() =>
            {
                if (_matchDetailsForm != null && !_matchDetailsForm.IsDisposed)
                {
                    _lastMatchDetailsBounds = _matchDetailsForm.Bounds;

                    _matchDetailsForm.Hide();

                    _matchDetailsForm.Dispose();
                    _matchDetailsForm = null;
                }

                _matchDetailsForm = new MatchDetails();

                if (!_lastMatchDetailsBounds.IsEmpty)
                {
                    _matchDetailsForm.StartPosition = FormStartPosition.Manual;
                    _matchDetailsForm.Bounds = _lastMatchDetailsBounds;
                }

                _matchDetailsForm.Show();
                _matchDetailsForm.BringToFront();
                _matchDetailsForm.Activate();
            });
        }

        private void DisplayMatch(MatchInfo matchInfo, CompetitiveTiersData? tiersData)
        {
            var map = matchInfo.MatchInfoInternal.MapId;
            var queueId = matchInfo.MatchInfoInternal.QueueID;
            var gameStart = matchInfo.MatchInfoInternal.GameStartMillis;

            var myPlayer = matchInfo.Players.FirstOrDefault(p => p.Subject == GlobalClient.UserId);
            if (myPlayer == null) return;

            var agentId = myPlayer.CharacterId;
            var kills = myPlayer.Stats.Kills;
            var deaths = myPlayer.Stats.Deaths;
            var assists = myPlayer.Stats.Assists;
            var score = myPlayer.Stats.Score;
            var tier = myPlayer.CompetitiveTier;

            Console.WriteLine($"Comp tier: {tier}");

            bool mvp = matchInfo.Players.All(p => p.Stats.Score <= score);

            var myTeam = matchInfo.Teams.FirstOrDefault(t => t.TeamId == myPlayer.TeamId);
            bool won = myTeam?.Won ?? false;

            long roundsWon = myTeam?.RoundsWon ?? 0;
            long roundsLost = myTeam?.RoundsPlayed - roundsWon ?? 0;

            AddMatchCardAsync(agentId, tier, kills, deaths, assists, won,
                              roundsWon, roundsLost, map, tiersData, mvp);
        }
        private async Task<(Image? MapImage, string? MapName)> GetMapImageAsync(string mapUrl)
        {
            if (string.IsNullOrEmpty(mapUrl)) return (null, null);

            try
            {
                var mapsData = await RadiantConnect.ValorantApi.Maps.GetMapsAsync();
                if (mapsData?.Data == null) return (null, null);

                var mapDatum = mapsData.Data.FirstOrDefault(m => m.MapUrl == mapUrl);
                if (mapDatum == null) return (null, null);

                var image = await ImageHelper.DownloadImageAsync(mapDatum.ListViewIcon);
                return (image, mapDatum.DisplayName);
            }
            catch
            {
                return (null, null);
            }
        }

        private async Task AddMatchCardAsync(
    string agentId, long? tier, long? kills, long? deaths, long? assists,
    bool won, long roundsWon, long roundsLost, string mapId,
    CompetitiveTiersData? tiersData, bool mvp)
        {
            int panelWidth = flowGames.ClientSize.Width - 10;
            int panelHeight = 80;

            var rankImage = await ImageHelper.GetTierImageAsync(tier ?? 0, tiersData);
            var agentImage = await ImageHelper.GetAgentImageAsync(agentId);
            var (mapImage, _) = await GetMapImageAsync(mapId);

            var matchCard = new Panel
            {
                Width = panelWidth,
                Height = panelHeight,
                Margin = new Padding(5),
                BackColor = Color.Transparent
            };

            matchCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                if (mapImage != null)
                {
                    float imageRatio = (float)mapImage.Width / mapImage.Height;
                    float panelRatio = (float)panelWidth / panelHeight;

                    Rectangle drawRect;

                    if (imageRatio > panelRatio)
                    {
                        int scaledWidth = (int)(panelHeight * imageRatio);
                        int xOffset = (scaledWidth - panelWidth) / 2;
                        drawRect = new Rectangle(-xOffset, 0, scaledWidth, panelHeight);
                    }
                    else
                    {
                        int scaledHeight = (int)(panelWidth / imageRatio);
                        int yOffset = (scaledHeight - panelHeight) / 2;
                        drawRect = new Rectangle(0, -yOffset, panelWidth, scaledHeight);
                    }

                    e.Graphics.DrawImage(mapImage, drawRect);
                }

                Color overlayColor = won
                    ? Color.FromArgb(90, 34, 197, 94) 
                    : Color.FromArgb(90, 239, 68, 68); 

                using var overlayBrush = new SolidBrush(overlayColor);
                e.Graphics.FillRectangle(overlayBrush, matchCard.ClientRectangle);

                if (mvp)
                {
                    using var borderPen = new Pen(Color.Gold, 2);
                    e.Graphics.DrawRectangle(borderPen, new Rectangle(1, 1, matchCard.Width - 3, matchCard.Height - 3));
                }
            };

            int padding = 10;
            int iconSize = 50;

            // --- Agent Icon ---
            if (agentImage != null)
            {
                matchCard.Controls.Add(new PictureBox
                {
                    Image = agentImage,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(iconSize, iconSize),
                    Location = new Point(padding, (panelHeight - iconSize) / 2),
                    BackColor = Color.Transparent
                });
            }

            // --- Rank Icon ---
            if (rankImage != null)
            {
                matchCard.Controls.Add(new PictureBox
                {
                    Image = rankImage,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(iconSize, iconSize),
                    Location = new Point(padding + iconSize + 10, (panelHeight - iconSize) / 2),
                    BackColor = Color.Transparent
                });
            }

            int textStartX = padding + 2 * (iconSize + 10);

            // --- KDA ---
            matchCard.Controls.Add(new Label
            {
                Text = $"KDA: {kills}/{deaths}/{assists}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(textStartX, 15)
            });

            // --- Victory/Defeat + Scoreline ---
            var lblResult = new Label
            {
                Text = won ? "Victory" : "Defeat",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            matchCard.Controls.Add(lblResult);

            var lblScoreline = new Label
            {
                Text = $"{roundsWon} - {roundsLost}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            matchCard.Controls.Add(lblScoreline);

            lblResult.Location = new Point((panelWidth - lblResult.Width) / 2, 15);
            lblScoreline.Location = new Point((panelWidth - lblScoreline.Width) / 2, 40);

            flowGames.Controls.Add(matchCard);
        }

        private async void Main_Shown(object sender, EventArgs e)
        {
            loadingPanel.Visible = true;
            try
            {
                // --- Seasons ---
                SeasonsData? seasons = await GetSeasonsAsync();

                if (seasons?.Data != null && seasons.Data.Any())
                {
                    var latestSeason = seasons.Data
                        .Where(s => s.StartTime.HasValue)
                        .MaxBy(s => s.StartTime!.Value);

                    if (latestSeason != null)
                    {
                        Console.WriteLine($"Latest Start Time: {latestSeason.StartTime}");
                        Console.WriteLine($"ParentUuid: {latestSeason.Uuid}");
                        GlobalClient.SeasonId = latestSeason.Uuid;
                    }
                    else
                    {
                        Console.WriteLine("No valid seasons found.");
                    }
                }

                tsAuth.Text = $"Auth Method: {GlobalClient.AuthMethod}";
                string cacheFolder = Path.Combine(Application.StartupPath, "cache");
                if (!Directory.Exists(cacheFolder)) Directory.CreateDirectory(cacheFolder);

                string? playerName = await RConnectMethods.GetRiotIdByPuuidAsync(_initiator, GlobalClient.UserId);
                var playerComp = await _initiator.Endpoints.PvpEndpoints.FetchCompetitveUpdatesAsync(GlobalClient.UserId);

                flowGames.Controls.Clear();

                // --- Get competitive tiers once ---
                CompetitiveTiersData? tiersData = await GetCompetitiveTiersAsync();

                // --- Display up to 5 matches safely ---
                int desiredMatchCount = 5;
                int displayedMatches = 0;

                if (playerComp?.Matches != null && playerComp.Matches.Count > 0)
                {
                    int matchIndex = 0;

                    while (displayedMatches < desiredMatchCount && matchIndex < playerComp.Matches.Count)
                    {
                        var matchData = playerComp.Matches[matchIndex];
                        matchIndex++;

                        try
                        {
                            var matchInfo = await _initiator.Endpoints.PvpEndpoints.FetchMatchInfoAsync(matchData.MatchID);

                            if (matchInfo != null)
                            {
                                DisplayMatch(matchInfo, tiersData);
                                displayedMatches++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to load match {matchData.MatchID}: {ex.Message}");
                            // DO NOT add error panel
                            // just skip this match
                        }

                        await Task.Delay(2000);
                    }
                }

                // --- No matches found ---
                if (displayedMatches == 0)
                {
                    var lblNoMatches = new MaterialLabel
                    {
                        Text = "No Competitive matches found",
                        Font = new Font("Roboto", 12, FontStyle.Bold),
                        ForeColor = Color.White,
                        AutoSize = true,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    flowGames.Controls.Add(lblNoMatches);

                    void CenterLabel()
                    {
                        lblNoMatches.Location = new Point(
                            (flowGames.ClientSize.Width - lblNoMatches.Width) / 2,
                            (flowGames.ClientSize.Height - lblNoMatches.Height) / 2
                        );
                    }

                    flowGames.Layout += (s, ev) => CenterLabel();
                    CenterLabel();
                }

                // --- Player info ---
                var playerLoadout = await _initiator.Endpoints.PvpEndpoints.FetchPlayerLoadoutAsync();
                var firstMatch = playerComp?.Matches.FirstOrDefault();
                var playerRankTier = firstMatch?.TierAfterUpdate ?? 0;
                int playerRR = (int)(firstMatch?.RankedRatingAfterUpdate ?? 0);
                var playerCardUuid = playerLoadout?.Identity.PlayerCardID;

                lblRR.Text = $"{playerRR}/100 RR";
                progRR.Maximum = 100;
                progRR.Value = Math.Max(0, Math.Min(100, playerRR));

                if (!string.IsNullOrEmpty(playerName))
                    this.Invoke(() => lblPlayerName.Text = playerName);

                // --- Tier Icon using helper function ---
                if (tiersData?.Data != null && playerRankTier > 0)
                {
                    var tierImage = await ImageHelper.GetTierImageAsync(playerRankTier, tiersData);
                    if (tierImage != null)
                        this.Invoke(() => imgRank.Image = tierImage);
                }

                // --- Player Card ---
                if (!string.IsNullOrEmpty(playerCardUuid))
                {
                    PlayerCardsData? playerCardsData = await GetPlayerCardsAsync();
                    var playerCardEntry = playerCardsData?.Data.FirstOrDefault(c => c.Uuid == playerCardUuid);
                    if (!string.IsNullOrEmpty(playerCardEntry.LargeArt))
                    {
                        try
                        {
                            using var http = new HttpClient();
                            byte[] imageBytes = await http.GetByteArrayAsync(playerCardEntry.LargeArt);

                            using var ms = new MemoryStream(imageBytes);
                            Image cardImage = Image.FromStream(ms);

                            this.Invoke(() => imgCard.Image = cardImage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to load player card image: {ex.Message}");
                        }
                    }
                }

                // --- Storefront & Skins ---
                Storefront? storefront = await _initiator.Endpoints.StoreEndpoints.FetchStorefrontAsync();
                BalancesMain? balances = await _initiator.Endpoints.StoreEndpoints.FetchBalancesAsync();
                var singleItemOffers = storefront?.SkinsPanelLayout?.SingleItemStoreOffers;

                if (singleItemOffers != null && balances != null)
                {
                    WeaponsData? weaponsData = await GetWeaponsAsync();

                    MaterialLabel[] lblShops = { lblShop1, lblShop2, lblShop3, lblShop4 };
                    MaterialLabel[] lblPrices = { lblShopPrice1, lblShopPrice2, lblShopPrice3, lblShopPrice4 };
                    PictureBox[] imgs = { imgShop1, imgShop2, imgShop3, imgShop4 };

                    lblVP.Text = balances.Balances.ValorantPoints.ToString();
                    lblRad.Text = balances.Balances.Radianite.ToString();
                    lblKNG.Text = balances.Balances.KingdomCredits.ToString();

                    int i = 0;
                    foreach (var offer in singleItemOffers.Take(4))
                    {
                        string? itemId = offer.Rewards?.FirstOrDefault()?.ItemID;
                        long? valorantPoints = offer.Cost?.ValorantPoints;
                        if (itemId == null || valorantPoints == null) continue;

                        SkinLevelDatum? matchedLevel = null;
                        SkinDatum? parentSkin = null;

                        foreach (var weapon in weaponsData!.Data)
                        {
                            foreach (var skin in weapon.Skins)
                            {
                                matchedLevel = skin.Levels.FirstOrDefault(l => l.Uuid == itemId);
                                if (matchedLevel != null)
                                {
                                    parentSkin = skin;
                                    break;
                                }
                            }
                            if (matchedLevel != null) break;
                        }

                        if (matchedLevel == null || parentSkin == null) continue;

                        string displayName = parentSkin.DisplayName;
                        string displayIcon = matchedLevel.DisplayIcon;

                        Image? loadedImage = null;

                        if (!string.IsNullOrEmpty(displayIcon))
                        {
                            try
                            {
                                using var http = new HttpClient();
                                byte[] imageBytes = await http.GetByteArrayAsync(displayIcon);
                                using var ms = new MemoryStream(imageBytes);
                                loadedImage = Image.FromStream(ms);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to load shop image: {ex.Message}");
                            }
                        }

                        int index = i;
                        this.Invoke(() =>
                        {
                            lblShops[index].Text = displayName;
                            lblPrices[index].Text = $"{valorantPoints} VP";
                            if (loadedImage != null) imgs[index].Image = loadedImage;
                        });

                        i++;
                    }
                }


                // --- Socket ---
                if (GlobalClient.AuthMethod == "VALORANT")
                {
                    //chkMatch.Visible = true;
                    _socketManager = new SocketManager(_initiator);

                    _socketManager.OnMessageReceived += (loopState) =>
                    {
                        if (loopState == "PREGAME" || loopState == "INGAME")
                        {
                            Console.WriteLine($"Player is {loopState}");
                            if (chkMatch.Checked)
                            {
                                if (InvokeRequired)
                                    Invoke(RefreshMatchDetailsForm);
                                else
                                    RefreshMatchDetailsForm();
                            }
                        }
                    };

                    _socketManager.Initialize();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading player info: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingPanel.Visible = false;
            }
        }


        
        private async void btnMatch_Click(object sender, EventArgs e)
        {
            if (GlobalClient.AuthMethod == "Riot Client")
            {
                MessageBox.Show("You need to restart the program with Valorant open", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CurrentGamePlayer currentGamePlayer = null;
            PreGamePlayer pregamePlayer = null;

            try { currentGamePlayer = await _initiator.Endpoints.CurrentGameEndpoints.GetCurrentGamePlayerAsync(); }
            catch { }
            try { pregamePlayer = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync(); }
            catch { }

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

        private void btnInstaLock_Click(object sender, EventArgs e)
        {
            if (GlobalClient.AuthMethod == "Riot Client")
            {
                MessageBox.Show("You need to restart the program with Valorant open", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var instaLock = new InstaLock();
                instaLock.Show();
            }
        }

        private async void btnDodge_Click(object sender, EventArgs e)
        {
            if (GlobalClient.AuthMethod == "Riot Client")
            {
                MessageBox.Show("You need to restart the program with Valorant open", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var match = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync();

                if (match?.MatchId == null)
                {
                    MessageBox.Show("You are not in a pre-game lobby.", "Cannot Dodge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await _initiator.Endpoints.PreGameEndpoints.QuitGameAsync();
                MessageBox.Show("You have left the pre-game lobby.", "Dodge Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (RadiantConnect.RadiantConnectNetworkStatusException ex)
            {
                if (ex.Message.Contains("RESOURCE_NOT_FOUND"))
                    MessageBox.Show("You are not in a pre-game lobby.", "Cannot Dodge", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show($"Failed to leave lobby: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to leave lobby: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class MatchEventArgs : EventArgs
    {
        public string MatchId { get; }
        public MatchEventArgs(string matchId)
        {
            MatchId = matchId;
        }
    }
}
