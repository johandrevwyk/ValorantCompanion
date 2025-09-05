using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.ValorantApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RadiantConnect.Methods.ValorantTables;

namespace ValorantCompanion
{
    public partial class InstaLock : MaterialForm
    {
        private Initiator _initiator;
        private string _selectedAgentUUID = null; // Stores display name
        private FlowLayoutPanel flowAgent;

        // Simple in-memory cache for images
        private readonly Dictionary<string, Image> _imageCache = new Dictionary<string, Image>();

        private readonly string _cacheFolder = Path.Combine(Application.StartupPath, "cache");
        private readonly string _agentsJsonFile;

        public InstaLock()
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

            //Stop window from resizing
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // disables resizing border
            this.MaximizeBox = false;                            // disable maximize
            this.MinimizeBox = false;                            // optional: set true if you want minimize
            this.SizeGripStyle = SizeGripStyle.Hide;             // hide size grip

            var fixedSize = this.Size;
            this.MinimumSize = fixedSize;
            this.MaximumSize = fixedSize;


            this.StartPosition = FormStartPosition.CenterScreen;

            _agentsJsonFile = Path.Combine(_cacheFolder, "agents.json");

            InitializeLayout();
            LoadAgentsAsync();
        }

        private void InitializeLayout()
        {
            // Flow panel for agents
            flowAgent = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            Controls.Add(flowAgent);

            // Confirm button
            var btnConfirm = new MaterialButton
            {
                Text = "Confirm Selection",
                Dock = DockStyle.Bottom,
                Height = 50
            };

            btnConfirm.Click += async (s, e) =>
            {
                if (_selectedAgentUUID == null)
                {
                    MessageBox.Show("Please select an agent before confirming.", "No Agent Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                flowAgent.Visible = false;
                btnConfirm.Hide();

                // Waiting panel
                var waitingPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(30, 30, 30),
                };
                Controls.Add(waitingPanel);
                waitingPanel.BringToFront();

                // Agent image
                if (_imageCache.ContainsKey(_selectedAgentUUID))
                {
                    var pic = new PictureBox
                    {
                        Image = _imageCache[_selectedAgentUUID],
                        Width = 150,
                        Height = 150,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Dock = DockStyle.Top,
                        Margin = new Padding(0, 20, 0, 20)
                    };
                    waitingPanel.Controls.Add(pic);
                }

                // Waiting label
                var lblWaiting = new Label
                {
                    Text = "Waiting for game to start",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                waitingPanel.Controls.Add(lblWaiting);

                try
                {
                    bool locked = false;

                    while (!locked)
                    {
                        try
                        {
                            var match = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync(GlobalClient.UserId);
                            if (match?.MatchId != null)
                            {
                                Console.WriteLine($"[INSTALOCK] Found match: {match.MatchId}, locking agent...");

                                // Parse selected agent string to enum
                                if (Enum.TryParse<RadiantConnect.Methods.ValorantTables.Agent>(_selectedAgentUUID, out var agentEnum))
                                {
                                    await _initiator.Endpoints.PreGameEndpoints.SelectCharacterAsync(match.MatchId, agentEnum);
                                    await _initiator.Endpoints.PreGameEndpoints.LockCharacterAsync(match.MatchId, agentEnum);
                                    Console.WriteLine("[INSTALOCK] Agent locked successfully.");
                                    locked = true;

                                    // Close form on UI thread
                                    this.Invoke(Close);
                                }
                                else
                                {
                                    Console.WriteLine($"[INSTALOCK] Could not parse '{_selectedAgentUUID}' to RadiantConnect.Methods.ValorantTables.Agent enum.");
                                }
                            }
                        }
                        catch
                        {
                            // Ignore errors (match not available yet)
                        }

                        if (!locked)
                            await Task.Delay(2000); // Retry after 2 seconds
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to lock agent: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Controls.Add(btnConfirm);
            flowAgent.BringToFront();
        }

        private async Task<string> GetAgentsJsonAsync()
        {
            if (!Directory.Exists(_cacheFolder))
                Directory.CreateDirectory(_cacheFolder);

            if (File.Exists(_agentsJsonFile))
                return await File.ReadAllTextAsync(_agentsJsonFile);

            using var http = new HttpClient();
            string json = await http.GetStringAsync("https://valorant-api.com/v1/agents");
            await File.WriteAllTextAsync(_agentsJsonFile, json);
            return json;
        }

        private async Task LoadAgentsAsync()
        {
            try
            {
                using var http = new HttpClient();
                string json = await GetAgentsJsonAsync();
                var jsonDoc = JsonDocument.Parse(json);
                var agents = jsonDoc.RootElement.GetProperty("data")
                    .EnumerateArray()
                    .Where(a => a.GetProperty("isPlayableCharacter").GetBoolean());

                flowAgent.Controls.Clear();
                var tasks = new List<Task>();

                foreach (var agent in agents)
                {
                    string uuid = agent.GetProperty("uuid").GetString();
                    string name = agent.GetProperty("displayName").GetString();
                    string iconUrl = agent.GetProperty("displayIcon").GetString();

                    var panel = new Panel
                    {
                        Width = 100,
                        Height = 130,
                        Margin = new Padding(10),
                        BackColor = Color.FromArgb(45, 45, 45)
                    };

                    var pic = new PictureBox
                    {
                        Width = 80,
                        Height = 80,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Cursor = Cursors.Hand,
                        Tag = name
                    };

                    var radio = new RadioButton
                    {
                        Text = name,
                        Width = 100,
                        Height = 30,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Appearance = Appearance.Button,
                        FlatStyle = FlatStyle.Flat,
                        ForeColor = Color.White,
                        Tag = name,
                        Top = 85
                    };

                    pic.Click += (s, e) => { radio.Checked = true; };
                    radio.CheckedChanged += (s, e) =>
                    {
                        if (radio.Checked)
                            _selectedAgentUUID = name;
                    };

                    panel.Controls.Add(pic);
                    panel.Controls.Add(radio);
                    flowAgent.Controls.Add(panel);

                    // Load images async
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            Image img;
                            if (_imageCache.ContainsKey(name))
                                img = _imageCache[name];
                            else
                            {
                                string cachedFile = Path.Combine(_cacheFolder, $"{uuid}.png");
                                if (File.Exists(cachedFile))
                                    img = Image.FromFile(cachedFile);
                                else
                                {
                                    using var stream = await http.GetStreamAsync(iconUrl);
                                    img = Image.FromStream(stream);
                                    img.Save(cachedFile);
                                }

                                _imageCache[name] = img;
                            }

                            pic.Invoke((Action)(() => pic.Image = img));
                        }
                        catch { }
                    }));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load agents: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
