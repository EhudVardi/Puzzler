using PresentationLogic;
using PresentationLogic.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Common;

namespace Presentation.WPF
{
    public partial class MainWindow : Window
    {
        internal static PresentationLogicBase PresentationLogicObject;

        public MainWindow()
        {
            InitializeComponent();
            this.ucDataGridGenerator.RequestLoadPuzzle += ucDataGrid_RequestLoadPuzzle;
            this.ucDataGridText.RequestLoadPuzzle      += ucDataGrid_RequestLoadPuzzle;
            this.ucDataGridWeb.RequestLoadPuzzle       += ucDataGrid_RequestLoadPuzzle;
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
                    case "Sudoku":        PresentationLogicObject = new PresentationLogicSudoku();        break;
                    case "Kakuru":        PresentationLogicObject = new PresentationLogicKakuru();        break;
                    case "Griddler":      PresentationLogicObject = new PresentationLogicGriddler();      break;
                    case "Griddler Rails":PresentationLogicObject = new PresentationLogicGriddlerRails(); break;
                    case "Triddler":      PresentationLogicObject = new PresentationLogicTriddler();      break;
                    default:              throw new Exception();
                }

                if (PresentationLogicObject != null)
                {
                    Dictionary<string, List<string>> puzzlesDic = PresentationLogicObject.ReadFileList();
                    this.ucDataGridGenerator.SetData(puzzlesDic[Configuration.FromGeneratorFolder]);
                    this.ucDataGridText.SetData(puzzlesDic[Configuration.FromTextFolder]);
                    this.ucDataGridWeb.SetData(puzzlesDic[Configuration.FromWebFolder]);

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
            RefreshForm(e);
        }

        private void RefreshForm()               => RefreshForm(EventArgs.Empty);
        private void RefreshForm(EventArgs e)
        {
            if (PresentationLogicObject != null)
            {
                this.GameCanvas.InvalidateVisual();
                try
                {
                    this.lblStatus.Text = string.Format("Valid: {0}, Solved: {1}, Progress: {2}",
                        PresentationLogicObject.IsValid(),
                        PresentationLogicObject.IsSolved(),
                        (e is System.ComponentModel.ProgressChangedEventArgs pce)
                            ? "Solving..." + pce.ProgressPercentage : "Idle");
                    this.lblStatusTitle.Text = "Status: OK";
                }
                catch (Exception)
                {
                    this.lblStatusTitle.Text = "Status: ERR";
                }
            }
        }

        private void ResizeWindowForCurrentPuzzle()
        {
            var size = PresentationLogicObject.GetPrefferedSize();
            int preferedSWidth = (int)(this.GameCanvas.ActualHeight * size.Width / size.Height);
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
            if (PresentationLogicObject == null) return;
            var location = e.GetPosition(this.GameCanvas);
            var pev = new PointerEvent((float)location.X, (float)location.Y, PointerButton.Left);
            PresentationLogicObject.HandlePointer(pev, (float)GameCanvas.ActualWidth, (float)GameCanvas.ActualHeight);
            RefreshForm();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e) { }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e) { }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (PresentationLogicObject == null) return;
            var pev = new PointerEvent(0, 0, PointerButton.None, e.Delta);
            PresentationLogicObject.HandlePointerWheel(pev, (float)GameCanvas.ActualWidth, (float)GameCanvas.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (PresentationLogicObject == null) return;
            int keyValue = KeyInterop.VirtualKeyFromKey(e.Key);
            PresentationLogicObject.HandleKey(new KeyEvent(keyValue));
            RefreshForm();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RefreshForm();
        }

        private void rbtnDisplayModes_Checked(object sender, RoutedEventArgs e)
        {
            if (PresentationLogicObject == null) return;
            try
            {
                if      (rbtnClean.IsChecked  == true) PresentationLogicObject.ShowBoard();
                else if (rbtnHints.IsChecked  == true) PresentationLogicObject.ShowHints();
                else if (rbtnSolved.IsChecked == true) PresentationLogicObject.ShowSolution();
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
            inputWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
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
