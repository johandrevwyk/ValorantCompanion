using RadiantConnect;
using RadiantConnect.Authentication.DriverRiotAuth.Records;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ValorantCompanion
{
    public static class GlobalClient
    {
        public static Initiator Initiator { get; set; } = null!;
        public static string GlzUrl { get; set; } = string.Empty;
        public static string PdUrl { get; set; } = string.Empty;
        public static string SharedUrl { get; set; } = string.Empty;
        public static string UserId { get; set; } = string.Empty;
    }

    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [STAThread]
        static void Main()
        {

            // Allocate a console for debugging
            //AllocConsole();
            Console.WriteLine("Global debug console initialized.");

            // Check if Valorant is running
            if (!IsProcessRunning("VALORANT")) // Replace with the actual process name
            {
                MessageBox.Show(
                    "Please start Valorant before running this companion app.",
                    "Valorant Not Running",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return; // Exit program
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start with Auth form
            Application.Run(new Auth());
        }

        private static bool IsProcessRunning(string processName)
        {
            // Check all running processes for a name match
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
    }
}
