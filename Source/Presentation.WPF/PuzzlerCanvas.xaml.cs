using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Presentation.WPF
{
    public partial class PuzzlerCanvas : UserControl
    {
        public PuzzlerCanvas()
        {
            InitializeComponent();
        }

        private bool _isRendering;

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!_isRendering)
            {
                _isRendering = true;
                try
                {
                    if (MainWindow.PresentationLogicObject != null)
                    {
                        var surface = new WpfDrawingSurface(drawingContext);
                        MainWindow.PresentationLogicObject.Draw(surface, (float)ActualWidth, (float)ActualHeight);
                        base.OnRender(drawingContext);
                    }
                }
                finally
                {
                    _isRendering = false;
                }
            }
        }
    }
}
