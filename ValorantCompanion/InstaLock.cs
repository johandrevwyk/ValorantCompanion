using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.ValorantApi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RadiantConnect.Methods.ValorantTables;
using static RadiantConnect.ValorantApi.Agents;

namespace ValorantCompanion
{
    public partial class InstaLock : MaterialForm
    {
        private Initiator _initiator;
        private string _selectedAgentUUID = null;
        private FlowLayoutPanel flowAgent;
        private SocketManager _socketManager;
        private bool _confirmClicked = false;

        private readonly Dictionary<string, Image> _imageCache = new Dictionary<string, Image>();
        private readonly string _cacheFolder = Path.Combine(Application.StartupPath, "cache");
        private readonly string _agentsJsonFile;

        public InstaLock()
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
            flowAgent = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            Controls.Add(flowAgent);

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

                _confirmClicked = true;

                flowAgent.Visible = false;
                btnConfirm.Hide();

                var waitingPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(30, 30, 30),
                };
                Controls.Add(waitingPanel);
                waitingPanel.BringToFront();

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

                await StartInstalockLoop();

                if (_socketManager == null)
                    InitializeSocketListener();
            };

            Controls.Add(btnConfirm);
            flowAgent.BringToFront();
        }

        private async Task StartInstalockLoop()
        {
            try
            {
                while (_confirmClicked)
                {
                    try
                    {
                        var match = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync();
                        if (match?.MatchId != null)
                        {
                            Console.WriteLine($"[INSTALOCK] Found match: {match.MatchId}, locking agent...");

                            Agents.Agent? agentData = await GetAgentAsync(_selectedAgentUUID);
                            if (agentData == null) continue;

                            if (Enum.TryParse<RadiantConnect.Methods.ValorantTables.Agent>(
                                    agentData.Data!.DisplayName.Replace(" ", ""),
                                    out var agentEnum))
                            {
                                await _initiator.Endpoints.PreGameEndpoints.SelectCharacterAsync(agentEnum);
                                await _initiator.Endpoints.PreGameEndpoints.LockCharacterAsync(agentEnum);
                                Console.WriteLine("[INSTALOCK] Agent locked successfully.");

                                this.Invoke(Close);
                                break;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to lock agent: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void InitializeSocketListener()
        {
            _socketManager = new SocketManager(_initiator);
            _socketManager.OnMessageReceived += async (loopState) =>
            {
                if (loopState == "PREGAME" && _confirmClicked)
                {
                    Console.WriteLine("Socket detected PREGAME, attempting to lock...");
                    await LockAgentAsync();
                }
            };
            _socketManager.Initialize();
        }

        private async Task LockAgentAsync()
        {
            if (!_confirmClicked) return;

            try
            {
                var match = await _initiator.Endpoints.PreGameEndpoints.FetchPreGamePlayerAsync();
                if (match?.MatchId == null) return;

                Console.WriteLine($"[INSTALOCK] Found match: {match.MatchId}, locking agent...");

                Agents.Agent? agentData = await GetAgentAsync(_selectedAgentUUID);
                if (agentData == null) return;

                if (Enum.TryParse<RadiantConnect.Methods.ValorantTables.Agent>(
                        agentData.Data!.DisplayName.Replace(" ", ""),
                        out var agentEnum))
                {
                    await _initiator.Endpoints.PreGameEndpoints.SelectCharacterAsync(agentEnum);
                    await _initiator.Endpoints.PreGameEndpoints.LockCharacterAsync(agentEnum);
                    Console.WriteLine("[INSTALOCK] Agent locked successfully.");
                    this.Invoke(Close);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INSTALOCK] Error while locking agent: {ex.Message}");
            }
        }

        private async Task LoadAgentsAsync()
        {
            try
            {
                AgentsData? agentsData = await GetAgentsAsync();
                if (agentsData?.Data == null) return;

                var playableAgents = agentsData.Data
                    .Where(a => a.IsPlayableCharacter.GetValueOrDefault())
                    .ToList();

                flowAgent.Controls.Clear();
                var tasks = new List<Task>();

                foreach (var agent in playableAgents)
                {
                    string uuid = agent.Uuid;
                    string name = agent.DisplayName;

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
                            var img = await ImageHelper.GetAgentImageAsync(uuid);
                            if (img != null)
                            {
                                _imageCache[uuid] = img;
                                pic.Invoke((Action)(() => pic.Image = img));
                            }
                        }
                        catch
                        {
                        }
                    }));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load agents: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _confirmClicked = false;
            base.OnFormClosing(e);
        }
    }
}
