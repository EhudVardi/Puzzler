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
        internal static PresentationLogicBase PresentationLogicObject;
        
        public MainWindow()
        {
            InitializeComponent();
            this.ucDataGridGenerator.RequestLoadPuzzle += ucDataGrid_RequestLoadPuzzle;
            this.ucDataGridText.RequestLoadPuzzle += ucDataGrid_RequestLoadPuzzle;
            this.ucDataGridWeb.RequestLoadPuzzle += ucDataGrid_RequestLoadPuzzle;
        }
        void ucDataGrid_RequestLoadPuzzle(object sender, ucPuzzlerDataGrid.RequestLoadPuzzleEventArgs e)
        {
            PresentationLogicObject.ReadFromFile(e.Path); 
            ResizeWindowForCurrentPuzzle();
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
                    Dictionary<string, List<string>> puzzlesDic = PresentationLogicObject.ReadFileList();
                    this.ucDataGridGenerator.SetData(puzzlesDic["FromGenerator"]);
                    this.ucDataGridText.SetData(puzzlesDic["FromText"]);
                    this.ucDataGridWeb.SetData(puzzlesDic["FromWeb"]);

                    this.GameCanvas.HookEventsToPresentationLogicObject();
                    PresentationLogicObject.Initialize();
                    PresentationLogicObject.Refresh += PresentationLogicObject_Refresh;
                    rbtnDisplayModes_Checked(null, null);
                }

                RefreshForm();
            }
            catch (Exception) { }
        }

        void PresentationLogicObject_Refresh(object sender, EventArgs e)
        {
            RefreshForm();
        }

        private void RefreshForm()
        {
            if (PresentationLogicObject != null)
            {
                this.GameCanvas.InvalidateVisual();
                try
                {
                    this.lblStatus.Text = string.Format("Valid: {0}, Solved: {1}",
                        PresentationLogicObject.IsValid(),
                        PresentationLogicObject.IsSolved());
                    this.lblStatusTitle.Text = "Status:" + " OK";
                }
                catch (Exception) {
                    this.lblStatusTitle.Text = "Status:" + " ERR";
                }
            }
        }
        private void ResizeWindowForCurrentPuzzle()
        {
            System.Drawing.Size size = PresentationLogicObject.GetPrefferedSize();
            int preferedSWidth = (int)this.GameCanvas.ActualHeight * size.Width / size.Height;
            this.Width = this.Width + preferedSWidth - this.GameCanvas.ActualWidth;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.btnSelectSudoku.IsChecked = false;
            this.btnSelectSudoku.IsChecked = true;
            this.rbtnSolved.IsChecked = false;
            this.rbtnSolved.IsChecked = true;
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

        private void btnLoadFromWeb_Click(object sender, RoutedEventArgs e)
        {
            PresentationLogicObject.ReadFromWeb(null);
            RefreshForm();
        }
        private void btnLoadFromText_Click(object sender, RoutedEventArgs e)
        {
            InputWindow inputWindow = new InputWindow();
            inputWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            inputWindow.ShowDialog();
            if (inputWindow.DialogResult.HasValue && inputWindow.DialogResult.Value == true)
            {
                PresentationLogicObject.ReadFromText(inputWindow.Data);
                ResizeWindowForCurrentPuzzle();
                RefreshForm();
            }
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            PresentationLogicObject.GenerateRandom();
            RefreshForm();
        }

        private void dpnlTitleBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button b = e.Source as Button;
            if (b != null && object.Equals(b, btnExitApplication))
            {
                Application.Current.Shutdown();
            }
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}


