using MaterialSkin;
using MaterialSkin.Controls;
using RadiantConnect;
using RadiantConnect.RConnect;
using System;
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

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            try
            {

                // Initialize Initiator in background thread
                var initiator = await System.Threading.Tasks.Task.Run(() => new Initiator());

                // Store globally for Main
                GlobalClient.Initiator = initiator;
                GlobalClient.GlzUrl = initiator.Client.GlzUrl;
                GlobalClient.PdUrl = initiator.Client.PdUrl;
                GlobalClient.SharedUrl = initiator.Client.SharedUrl;
                GlobalClient.UserId = initiator.Client.UserId;

                Console.WriteLine("Initiator initialized successfully.");

                // Open Main form
                this.BeginInvoke(new Action(() =>
                {
                    var mainForm = new Main();
                    mainForm.FormClosed += (s, e) => Application.Exit();
                    mainForm.Show();

                    this.Hide(); // Close Auth completely
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
