using PresentationLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace Presentation.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSelectPuzzles_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = e.Source as RadioButton;
                switch (rb.Content as string)
                {
                    case "Sudoku":
                        PresentationLogicObject = new PresentationLogicSudoku();
                        break;
                    case "Kakuru":
                        PresentationLogicObject = new PresentationLogicKakuru();
                        break;
                    case "Griddler":
                        PresentationLogicObject = new PresentationLogicGriddler();
                        break;
                    case "Griddler Rails":
                        PresentationLogicObject = new PresentationLogicGriddlerRails();
                        break;
                    default:
                        throw new Exception();
                        break;
                }

                if (PresentationLogicObject != null)
                {
                    this.GameCanvas.HookEventsToPresentationLogicObject(PresentationLogicObject);
                    PresentationLogicObject.Initialize();
                }

                RefreshForm();
            }
            catch (Exception)
            {
            }
        }



        private void RefreshForm()
        {
            if (PresentationLogicObject != null)
            {
                this.GameCanvas.InvalidateVisual();
                //this.lblStatus.Text = string.Format("Valid: {0}, Solved: {1}",
                //    PresentationLogicObject.IsValid(),
                //    PresentationLogicObject.IsSolved());
            }
        }

        internal static PresentationLogicBase PresentationLogicObject;
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnSelectSudoku.IsChecked = false;
            this.btnSelectSudoku.IsChecked = true;
            this.rbtnClean.IsChecked = false;
            this.rbtnClean.IsChecked = true;
        }

        //private void frm_MainForm_Refresh(object sender, EventArgs e)
        //{
        //    RefreshForm();
        //}
        //private void frm_MainForm_KeyDown(object sender, KeyEventArgs e)
        //{
        //    presentationLogicObject.InputKey(e);
        //}
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RefreshForm();
        }
        private void rbtnDisplayModes_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rbtnClean.IsChecked == true)
                    PresentationLogicObject.ShowBoard();
                else if (rbtnHints.IsChecked == true)
                    PresentationLogicObject.ShowHints();
                else if (rbtnSolved.IsChecked == true)
                    PresentationLogicObject.ShowSolution();
                else { }
                RefreshForm();
            }
            catch (Exception) { }
        }

        private void btnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog opf = new Microsoft.Win32.OpenFileDialog();
            opf.InitialDirectory = System.IO.Path.GetFullPath(PresentationLogicObject.GetPuzzleTypeDocumentsPath() + Common.Configuration.PuzzlesLibraryFolder);
            opf.RestoreDirectory = false;
            opf.Multiselect = false;
            opf.Filter = "xml files (*.xml)|*.xml";

            bool? dr = opf.ShowDialog(this);
            if (dr != null && dr == true)
            {
                PresentationLogicObject.ReadFromFile(opf.FileName);
                RefreshForm();
            }
        }
        private void btnLoadFromWeb_Click(object sender, RoutedEventArgs e)
        {
            PresentationLogicObject.ReadFromWeb(null);
            RefreshForm();
        }
        private void btnLoadFromText_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("not implemented yet (need to create a form to paste text)");
            //    InputForm inputForm = new InputForm();
            //    inputForm.StartPosition = FormStartPosition.CenterParent;
            //    bool ok = false;
            //    while (!ok)
            //    {
            //        DialogResult dr = inputForm.ShowDialog();
            //        if (dr == DialogResult.OK)
            //        {
            //            if (presentationLogicObject.ReadFromText(inputForm.Data))
            //                ok = true;
            //        }
            //        else if (dr == DialogResult.Cancel)
            //            ok = true;
            //    }
        }
        
        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            PresentationLogicObject.GenerateRandom();
            RefreshForm();
        }


        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //System.Windows.Point location = e.GetPosition(this.GameCanvas);
            //System.Windows.Forms.MouseEventArgs mea = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, (int)location.X, (int)location.Y, 1);
            //MainWindow.PresentationLogicObject.InputMouse(mea, new System.Drawing.Size((int)this.GameCanvas.ActualWidth, (int)this.GameCanvas.ActualHeight));
            //RefreshForm();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point location = e.GetPosition(this.GameCanvas);
            System.Windows.Forms.MouseEventArgs mea = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, (int)location.X, (int)location.Y, 1);
            MainWindow.PresentationLogicObject.InputMouse(mea, new System.Drawing.Size((int)this.GameCanvas.ActualWidth, (int)this.GameCanvas.ActualHeight));
            RefreshForm();
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {

        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Forms.MouseEventArgs mea = new System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 0, 0, e.Delta);
            MainWindow.PresentationLogicObject.InputMouseWheel(mea, new System.Drawing.Size((int)this.GameCanvas.ActualWidth, (int)this.GameCanvas.ActualHeight));

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            MainWindow.PresentationLogicObject.InputKey(new System.Windows.Forms.KeyEventArgs(ConvertWinfromsObjects.ConvertKeys(e.Key)));
            RefreshForm();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshForm();
        }
    }
}


