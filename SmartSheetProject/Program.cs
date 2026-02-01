using System;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using SmartSheetProject.Forms;

namespace SmartSheetProject
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}