using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Presentation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //testing

            //

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            //Application.Run(new Presentation.Sudoku.SolverForm());
            //Application.Run(new Presentation.Kakuru.SolverForm());
            //Application.Run(new Presentation.Griddler.SolverForm());

            Application.Run(new frm_MainForm());
            
        }
    }
}