using System;
using System.Windows.Forms;
using SudokuBattleOnline.Forms;

namespace SudokuBattleOnline
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Application.Run(new LoginForm());
        }
    }
}