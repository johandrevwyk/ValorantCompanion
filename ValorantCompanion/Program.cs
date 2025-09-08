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
        public static string AuthMethod { get; set; } = string.Empty;
    }

    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [STAThread]
        static void Main()
        {

            // Allocate a console for debugging
            AllocConsole();
            Console.WriteLine("Global debug console initialized.");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start with Auth form
            Application.Run(new Auth());
        }
    }
}
