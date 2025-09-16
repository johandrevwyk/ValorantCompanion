using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.Authentication;
using RadiantConnect.Authentication.DriverRiotAuth.Records;
using RadiantConnect.Authentication.QRSignIn.Modules;
using RadiantConnect.RConnect;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;

namespace ValorantCompanion
{
    public partial class Auth : MaterialForm
    {
        private MaterialLabel _statusLabel;
        private MaterialProgressBar _spinner;

        // This flag will stop the main form from opening after a fatal error
        private bool _fatalErrorOccurred = false;

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
            this.BeginInvoke(new Action(async () => await InitializeAsync()));
        }

        private async Task InitializeAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    Initiator? initiator = null;

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

                        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        string riotSettingsPath = Path.Combine(localAppData, "Riot Games", "Riot Client", "Data", "RiotGamesPrivateSettings.yaml");

                        if (!File.Exists(riotSettingsPath))
                        {
                            ShowFatalError("Error using Riot Client Auth.\n\nMake sure you click 'Remember Me' when signing in to the Riot Client.");
                            return; 
                        }

                        string yaml = await File.ReadAllTextAsync(riotSettingsPath);
                        if (string.IsNullOrWhiteSpace(yaml))
                        {
                            ShowFatalError("Error using Riot Client Auth.\n\nYAML file is empty. Make sure you click 'Remember Me' when signing in to the Riot Client.");
                            return;
                        }

                        var deserializer = new Deserializer();
                        dynamic yamlObject;
                        try
                        {
                            yamlObject = deserializer.Deserialize<dynamic>(yaml);
                        }
                        catch (Exception ex)
                        {
                            ShowFatalError($"Error parsing Riot Client data: {ex.Message}");
                            return;
                        }

                        if (yamlObject == null || yamlObject["riot-login"] == null)
                        {
                            ShowFatalError("Invalid Riot Client data. Make sure you click 'Remember Me' when signing in or alternatively open Valorant first.");
                            return;
                        }

                        var session = yamlObject["riot-login"]?["persist"]?["session"];
                        if (session == null || session["cookies"] == null)
                        {
                            ShowFatalError("Riot Client session cookies not found. Make sure you click 'Remember Me' when signing in or alternatively open Valorant first.");
                            return;
                        }

                        var cookies = (IEnumerable<dynamic>)session["cookies"];

                        string? ssid = cookies.FirstOrDefault(c => (string)c["name"] == "ssid")?["value"];
                        string? clid = cookies.FirstOrDefault(c => (string)c["name"] == "clid")?["value"];
                        string? csid = cookies.FirstOrDefault(c => (string)c["name"] == "csid")?["value"];
                        string? asid = cookies.FirstOrDefault(c => (string)c["name"] == "asid")?["value"];
                        string? tdid = yamlObject["rso-authenticator"]?["tdid"]?["value"];

                        if (string.IsNullOrEmpty(ssid))
                        {
                            ShowFatalError("Error using Riot Client Auth.\n\nSSID cookie is missing. Make sure you click 'Remember Me' when signing in or alternatively open Valorant first.");
                            return;
                        }

                        var authentication = new Authentication();
                        RSOAuth? result = await authentication.AuthenticateWithSsid(ssid, clid, csid, tdid, asid);

                        GlobalClient.AuthMethod = "Riot Client";

                        if (result == null)
                        {
                            ShowFatalError("Riot Client authentication failed. Please try logging out and back in again with 'Remember Me' checked or alternatively open Valorant first.");
                            return;
                        }

                        initiator = new Initiator(result);
                    }

                    if (_fatalErrorOccurred || initiator == null) return;

                    GlobalClient.Initiator = initiator;
                    GlobalClient.GlzUrl = initiator.Client.GlzUrl;
                    GlobalClient.PdUrl = initiator.Client.PdUrl;
                    GlobalClient.SharedUrl = initiator.Client.SharedUrl;
                    GlobalClient.UserId = initiator.Client.UserId;

                    Console.WriteLine("Authentication complete. Initiator ready.");
                });

                if (_fatalErrorOccurred) return;

                this.BeginInvoke(new Action(() =>
                {
                    if (_fatalErrorOccurred) return;

                    var mainForm = new Main();
                    mainForm.FormClosed += (s, e) => Application.Exit();
                    mainForm.Show();
                    this.Hide();
                }));
            }
            catch (Exception ex)
            {
                ShowFatalError($"Authentication failed: {ex.Message}");
            }
        }

        private void ShowFatalError(string message)
        {
            _fatalErrorOccurred = true;

            this.BeginInvoke(new Action(() =>
            {
                MessageBox.Show(
                    message,
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // Close the app completely
                Application.Exit();
            }));
        }
    }
}
