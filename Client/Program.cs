using System;
using System.Windows.Forms;

namespace BordeauxRCClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        public static Core.Connection connection;
        public static Form1 mainForm;

        static internal string version = "Prerelease";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new Form1();
            Application.Run(mainForm);
            connection = new Core.Connection(mainForm);
        }
    }
}
