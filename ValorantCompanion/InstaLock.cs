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
using static RadiantConnect.ValorantApi.Agents;

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
                            var match = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync();
                            if (match?.MatchId != null)
                            {
                                Console.WriteLine($"[INSTALOCK] Found match: {match.MatchId}, locking agent...");

                                // Get the agent data from UUID
                                RadiantConnect.ValorantApi.Agents.Agent? agentData = await GetAgentAsync(_selectedAgentUUID);
                                if (agentData == null)
                                {
                                    Console.WriteLine("[INSTALOCK] Could not fetch agent data.");
                                    continue;
                                }

                                // Map display name (or developer name) to enum
                                if (Enum.TryParse<RadiantConnect.Methods.ValorantTables.Agent>(
                                        agentData.Data.DisplayName.Replace(" ", ""),
                                        out var agentEnum))
                                {
                                    await _initiator.Endpoints.PreGameEndpoints.SelectCharacterAsync(agentEnum);
                                    await _initiator.Endpoints.PreGameEndpoints.LockCharacterAsync(agentEnum);
                                    Console.WriteLine("[INSTALOCK] Agent locked successfully.");
                                    locked = true;

                                    this.Invoke(Close);
                                }
                                else
                                {
                                    Console.WriteLine($"[INSTALOCK] Could not map '{agentData.Data.DisplayName}' to Agent enum.");
                                }
                            }
                        }
                        catch
                        {
                            
                        }

                        if (!locked)
                            await Task.Delay(2000); 
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


        private async Task LoadAgentsAsync()
        {
            try
            {
                AgentsData? agentsData = await GetAgentsAsync();

                if (agentsData?.Data == null) return;

                var playableAgents = agentsData.Data
                    .Where(a => a.IsPlayableCharacter == true)
                    .ToList();

                flowAgent.Controls.Clear();
                var tasks = new List<Task>();

                foreach (var agent in playableAgents)
                {
                    string uuid = agent.Uuid;
                    string name = agent.DisplayName;
                    string iconUrl = agent.DisplayIcon;

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
                        Tag = uuid
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
                        Tag = uuid,
                        Top = 85
                    };

                    pic.Click += (s, e) => { radio.Checked = true; };
                    radio.CheckedChanged += (s, e) =>
                    {
                        if (radio.Checked)
                            _selectedAgentUUID = uuid; 
                    };

                    panel.Controls.Add(pic);
                    panel.Controls.Add(radio);
                    flowAgent.Controls.Add(panel);

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            Image img;
                            if (_imageCache.ContainsKey(uuid))
                            {
                                img = _imageCache[uuid];
                            }
                            else
                            {
                                string cachedFile = Path.Combine(_cacheFolder, $"{uuid}.png");
                                if (File.Exists(cachedFile))
                                {
                                    img = Image.FromFile(cachedFile);
                                }
                                else
                                {
                                    using var http = new HttpClient();
                                    using var stream = await http.GetStreamAsync(iconUrl);
                                    img = Image.FromStream(stream);
                                    img.Save(cachedFile);
                                }

                                _imageCache[uuid] = img;
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
