using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.Authentication;
using RadiantConnect.Authentication.DriverRiotAuth.Records;
using RadiantConnect.Authentication.QRSignIn.Modules;
using RadiantConnect.RConnect;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ValorantCompanion
{
    public partial class Auth : MaterialForm
    {
        private MaterialLabel _statusLabel;
        private MaterialProgressBar _spinner;

        public Auth()
        {
            InitializeComponent();
            Console.WriteLine("Debug console initialized.");

            SetupMaterialSkin();

            // Start Initiator initialization after form loads
            this.Load += Auth_Load;
        }

        private void SetupMaterialSkin()
        {
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
        }

        private void Auth_Load(object sender, EventArgs e)
        {
            // Begin async initialization without freezing UI
            this.BeginInvoke(new Action(async () => await InitializeAsync()));
        }

        private async Task InitializeAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    Initiator? initiator = null;

                    // Step 1: Check if Valorant is running
                    if (RConnectMethods.IsValorantRunning())
                    {
                        Console.WriteLine("Valorant process detected. Attempting to authenticate via running game...");
                        initiator = new Initiator();
                        GlobalClient.AuthMethod = "VALORANT";
                        if (initiator?.Client == null)
                        {
                            Console.WriteLine("Failed to initialize from running Valorant instance.");
                            initiator = null;
                        }
                    }

                    if (initiator == null)
                    {
                        Console.WriteLine("Authenticating via Riot Client (SSID)...");
                        // Load YAML, extract ssid, clid, csid, tdid, asid
                        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        string riotSettingsPath = Path.Combine(localAppData, "Riot Games", "Riot Client", "Data", "RiotGamesPrivateSettings.yaml");

                        if (!File.Exists(riotSettingsPath))
                            throw new FileNotFoundException("RiotGamesPrivateSettings.yaml not found.");

                        var yaml = await File.ReadAllTextAsync(riotSettingsPath);
                        var deserializer = new YamlDotNet.Serialization.Deserializer();
                        dynamic yamlObject = deserializer.Deserialize<dynamic>(yaml);

                        var cookies = (IEnumerable<dynamic>)yamlObject["riot-login"]["persist"]["session"]["cookies"];
                        string ssid = cookies.First(c => (string)c["name"] == "ssid")["value"];
                        string? clid = cookies.FirstOrDefault(c => (string)c["name"] == "clid")?["value"];
                        string? csid = cookies.FirstOrDefault(c => (string)c["name"] == "csid")?["value"];
                        string? asid = cookies.FirstOrDefault(c => (string)c["name"] == "asid")?["value"];
                        string? tdid = yamlObject["rso-authenticator"]?["tdid"]?["value"];

                        var authentication = new Authentication();
                        RSOAuth? result = await authentication.AuthenticateWithSsid(
                            ssid,
                            clid,
                            csid,
                            tdid,
                            asid
                        );

                        GlobalClient.AuthMethod = "Riot Client";

                        if (result == null)
                            throw new Exception("Riot Client authentication failed.");

                        initiator = new Initiator(result);
                    }

                    // Store initiator globally
                    GlobalClient.Initiator = initiator;
                    GlobalClient.GlzUrl = initiator.Client.GlzUrl;
                    GlobalClient.PdUrl = initiator.Client.PdUrl;
                    GlobalClient.SharedUrl = initiator.Client.SharedUrl;
                    GlobalClient.UserId = initiator.Client.UserId;

                    Console.WriteLine("Authentication complete. Initiator ready.");
                });

                // Back on UI thread
                this.BeginInvoke(new Action(() =>
                {
                    var mainForm = new Main();
                    mainForm.FormClosed += (s, e) => Application.Exit();
                    mainForm.Show();
                    this.Hide();
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during initialization: {ex}");
                MessageBox.Show($"Authentication failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Auth_Load_1(object sender, EventArgs e)
        {

        }
    }
}
