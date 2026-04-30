using PresentationLogic;
using PresentationLogic.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Common;

namespace Presentation.WPF
{
    public partial class MainWindow : Window
    {
        internal static PresentationLogicBase PresentationLogicObject = null!;
        private static readonly PuzzlerOptions _options = PuzzlerOptions.CreateDefault();

        public MainWindow()
        {
            InitializeComponent();
            this.ucDataGridGenerator.RequestLoadPuzzle += ucDataGrid_RequestLoadPuzzle;
            this.ucDataGridText.RequestLoadPuzzle      += ucDataGrid_RequestLoadPuzzle;
            this.ucDataGridWeb.RequestLoadPuzzle       += ucDataGrid_RequestLoadPuzzle;

            this.btnSelectSudoku.Tag   = PuzzleRegistry.Find("Sudoku");
            this.btnSelectKakuru.Tag   = PuzzleRegistry.Find("Kakuru");
            this.btnSelectGriddler.Tag = PuzzleRegistry.Find("Griddler");
            this.btnSelectTriddler.Tag = PuzzleRegistry.Find("Triddler");
        }

        void ucDataGrid_RequestLoadPuzzle(object sender, ucPuzzlerDataGrid.RequestLoadPuzzleEventArgs e)
        {
            try
            {
                PresentationLogicObject.ReadFromFile(e.Path);
                ResizeWindowForCurrentPuzzle();
                RecomputeBoardScale();
            }
            catch (IOException ex)  { ShowError(ex.Message); }
            catch (XmlException ex) { ShowError(ex.Message); }
        }

        private void btnSelectPuzzles_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton? rb = e.Source as RadioButton;
                IPuzzleDescriptor descriptor = rb?.Tag as IPuzzleDescriptor
                    ?? throw new InvalidOperationException($"No puzzle descriptor on '{rb?.Content}'");
                PresentationLogicObject = descriptor.Create();
                PresentationLogicObject.Options = _options;
                ((SkewTransform)((TransformGroup)GameCanvas.RenderTransform).Children[1]).AngleX = descriptor.BoardSkewAngle;

                Dictionary<string, List<string>>? puzzlesDic = PresentationLogicObject.ReadFileList();
                this.ucDataGridGenerator.SetData(ToPathSizeList(puzzlesDic?[_options.FromGeneratorFolder]));
                this.ucDataGridText.SetData(ToPathSizeList(puzzlesDic?[_options.FromTextFolder]));
                this.ucDataGridWeb.SetData(ToPathSizeList(puzzlesDic?[_options.FromWebFolder]));

                PresentationLogicObject.Initialize();
                PresentationLogicObject.Refresh += PresentationLogicObject_Refresh;
                rbtnDisplayModes_Checked(null, null);
                RefreshForm();
                RecomputeBoardScale();
            }
            catch (IOException ex)  { ShowError(ex.Message); }
            catch (XmlException ex) { ShowError(ex.Message); }
        }

        private List<(string path, string size)> ToPathSizeList(List<string>? paths)
        {
            if (paths == null) return new();
            return paths.ConvertAll(p => (p, PresentationLogicObject.GetPuzzleSizeLabel(p)));
        }

        void PresentationLogicObject_Refresh(object? sender, EventArgs e)
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
                catch (Exception ex)
                {
                    this.lblStatusTitle.Text = $"Status: ERR ({ex.GetType().Name})";
                }
            }
        }

        private void ResizeWindowForCurrentPuzzle()
        {
            var size = PresentationLogicObject.GetPrefferedSize();
            int preferedSWidth = (int)(this.GameCanvas.ActualHeight * size.Width / size.Height);
            this.Width = this.Width + preferedSWidth - this.GameCanvas.ActualWidth;
        }

        private void RecomputeBoardScale()
        {
            if (PresentationLogicObject == null) return;
            double w = GameCanvas.ActualWidth, h = GameCanvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            var transforms = ((TransformGroup)GameCanvas.RenderTransform).Children;
            var scaleTransform = (ScaleTransform)transforms[0];
            double skewRad = ((SkewTransform)transforms[1]).AngleX * Math.PI / 180.0;

            // DrawBoard already auto-fits content to the canvas size it's given. The
            // only thing the ScaleTransform needs to do is shrink to make headroom
            // for skew (Triddler), since skew widens the rendered bounding box.
            double tan = Math.Abs(Math.Tan(skewRad));
            double scale = tan < 1e-6 ? 1.0 : w / (w + tan * h);

            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
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
            RecomputeBoardScale();
            RefreshForm();
        }

        private void rbtnDisplayModes_Checked(object? sender, RoutedEventArgs? e)
        {
            if (PresentationLogicObject == null) return;
            if      (rbtnClean.IsChecked  == true) PresentationLogicObject.ShowBoard();
            else if (rbtnHints.IsChecked  == true) PresentationLogicObject.ShowHints();
            else if (rbtnSolved.IsChecked == true) PresentationLogicObject.ShowSolution();
            RefreshForm();
        }

        private void btnLoadFromWeb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PresentationLogicObject.ReadFromWeb(string.Empty);
                RefreshForm();
            }
            catch (WebException ex)         { ShowError(ex.Message); }
            catch (HttpRequestException ex)  { ShowError(ex.Message); }
            catch (IOException ex)           { ShowError(ex.Message); }
        }

        private void btnLoadFromText_Click(object sender, RoutedEventArgs e)
        {
            InputWindow inputWindow = new InputWindow();
            inputWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            inputWindow.ShowDialog();
            if (inputWindow.DialogResult.HasValue && inputWindow.DialogResult.Value == true)
            {
                try
                {
                    PresentationLogicObject.ReadFromText(inputWindow.Data);
                    ResizeWindowForCurrentPuzzle();
                    RecomputeBoardScale();
                    RefreshForm();
                }
                catch (FormatException ex)          { ShowError(ex.Message); }
                catch (IndexOutOfRangeException ex)  { ShowError("Malformed input: " + ex.Message); }
                catch (XmlException ex)              { ShowError(ex.Message); }
            }
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            PresentationLogicObject.GenerateRandom();
            RefreshForm();
        }

        private void dpnlTitleBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button? b = e.Source as Button;
            if (b != null && object.Equals(b, btnExitApplication))
            {
                Application.Current.Shutdown();
            }
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private static void ShowError(string message) =>
            MessageBox.Show(message, "Puzzler", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
