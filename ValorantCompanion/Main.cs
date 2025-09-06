using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.Network.CurrentGameEndpoints.DataTypes;
using RadiantConnect.Network.LocalEndpoints.DataTypes;
using RadiantConnect.Network.PreGameEndpoints.DataTypes;
using RadiantConnect.Network.StoreEndpoints.DataTypes;
using RadiantConnect.RConnect;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ValorantCompanion
{
    public partial class Main : MaterialForm
    {
        private Initiator _initiator;

        // Loading overlay
        private Panel loadingPanel;
        private MaterialProgressBar progressBar;
        private MaterialLabel loadingLabel;

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

            this.Shown += Main_Shown;

            // Stop window from resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;

            var fixedSize = this.Size;
            this.MinimumSize = fixedSize;
            this.MaximumSize = fixedSize;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Fix materialCard1 position/size
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
                // Force label to calculate its preferred size
                loadingLabel.AutoSize = true;
                loadingLabel.PerformLayout();
                loadingLabel.Refresh();

                // Center progress bar
                progressBar.Location = new Point(
                    (loadingPanel.ClientSize.Width - progressBar.Width) / 2,
                    (loadingPanel.ClientSize.Height - progressBar.Height) / 2
                );

                // Center label above progress bar
                loadingLabel.Location = new Point(
                    (loadingPanel.ClientSize.Width - loadingLabel.Width) / 2,
                    progressBar.Top - loadingLabel.Height - 10
                );
            }

            // Center after the panel has been fully laid out
            loadingPanel.Layout += (s, e) => CenterControls();

            // Also center when form resizes
            this.Resize += (s, e) => CenterControls();
        }




        private async void Main_Shown(object sender, EventArgs e)
        {
            loadingPanel.Visible = true;
            try
            {
                tsAuth.Text = $"Auth Method: {GlobalClient.AuthMethod}";
                string cacheFolder = Path.Combine(Application.StartupPath, "cache");
                if (!Directory.Exists(cacheFolder)) Directory.CreateDirectory(cacheFolder);

                string? playername = await RConnectMethods.GetRiotIdByPuuidAsync(_initiator, GlobalClient.UserId);
                var playercomp = await _initiator.Endpoints.PvpEndpoints.FetchCompetitveUpdatesAsync(GlobalClient.UserId);
                var playerloadout = await _initiator.Endpoints.PvpEndpoints.FetchPlayerLoadoutAsync();

                var playerranktier = playercomp.Matches.FirstOrDefault()?.TierAfterUpdate;
                var playerCardUuid = playerloadout.Identity.PlayerCardID;

                if (playername != null)
                {
                    this.Invoke(() => lblPlayerName.Text = $"{playername}");

                    // --- Competitive Tier ---
                    var tierDoc = await GlobalCache.LoadJsonWithCacheAsync(
                        "https://valorant-api.com/v1/competitivetiers/564d8e28-c226-3180-6285-e48a390db8b1",
                        cacheFolder
                    );
                    var tiersData = tierDoc.RootElement.GetProperty("data").GetProperty("tiers");

                    int playerTier = (int)playerranktier;
                    string tierIconUrl = null;

                    foreach (var tierEntry in tiersData.EnumerateArray())
                    {
                        if (tierEntry.GetProperty("tier").GetInt32() == playerTier)
                        {
                            tierIconUrl = tierEntry.GetProperty("largeIcon").GetString();
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(tierIconUrl))
                    {
                        var tierImage = await GlobalCache.LoadImageWithCacheAsync(tierIconUrl, cacheFolder);
                        this.Invoke(() => imgRank.Image = tierImage);
                    }

                    // --- Player Card ---
                    var cardDoc = await GlobalCache.LoadJsonWithCacheAsync("https://valorant-api.com/v1/playercards", cacheFolder);
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
                        var cardImage = await GlobalCache.LoadImageWithCacheAsync(cardUrl, cacheFolder);
                        this.Invoke(() => imgCard.Image = cardImage);
                    }

                    // --- Storefront & Skins ---
                    Storefront? storefront = await _initiator.Endpoints.StoreEndpoints.FetchStorefrontAsync();
                    var singleItemOffers = storefront.SkinsPanelLayout?.SingleItemStoreOffers;

                    if (singleItemOffers != null)
                    {
                        var weaponsDoc = await GlobalCache.LoadJsonWithCacheAsync("https://valorant-api.com/v1/weapons", cacheFolder);
                        var weaponsData = weaponsDoc.RootElement.GetProperty("data");

                        MaterialLabel[] lblShops = { lblShop1, lblShop2, lblShop3, lblShop4 };
                        MaterialLabel[] lblPrices = { lblShopPrice1, lblShopPrice2, lblShopPrice3, lblShopPrice4 };
                        PictureBox[] imgs = { imgShop1, imgShop2, imgShop3, imgShop4 };

                        int i = 0;
                        foreach (var offer in singleItemOffers.Take(4))
                        {
                            string? itemId = offer.Rewards?.FirstOrDefault()?.ItemID;
                            long? valorantPoints = offer.Cost?.ValorantPoints;
                            if (itemId == null || valorantPoints == null) continue;

                            bool found = false;
                            JsonElement matchedLevel = new JsonElement();
                            JsonElement parentSkin = new JsonElement();

                            foreach (var weapon in weaponsData.EnumerateArray())
                            {
                                foreach (var skin in weapon.GetProperty("skins").EnumerateArray())
                                {
                                    foreach (var level in skin.GetProperty("levels").EnumerateArray())
                                    {
                                        if (level.GetProperty("uuid").GetString() == itemId)
                                        {
                                            matchedLevel = level;
                                            parentSkin = skin;
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (found) break;
                                }
                                if (found) break;
                            }

                            if (!found) continue;

                            string displayName = parentSkin.GetProperty("displayName").GetString() ?? "";
                            string displayIcon = matchedLevel.GetProperty("displayIcon").GetString() ?? "";

                            Image? loadedImage = null;
                            if (!string.IsNullOrEmpty(displayIcon))
                                loadedImage = await GlobalCache.LoadImageWithCacheAsync(displayIcon, cacheFolder);

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

        private void Main_Load(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) { }
        private void materialCard1_Paint(object sender, PaintEventArgs e) { }
        private void lblShopPrice1_Click(object sender, EventArgs e) { }
        private void materialCard2_Paint(object sender, PaintEventArgs e) { }
    }
}
