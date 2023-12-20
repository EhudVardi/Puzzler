using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using PresentationLogic;
using System.Drawing;

namespace Presentation.WPF
{
    ////public static class ExtensionMethods
    ////{
    ////
    ////    private static Action EmptyDelegate = delegate() { };
    ////
    ////
    ////    public static void Refresh(this UIElement uiElement)
    ////    {
    ////        //uiElement.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Render);
    ////        uiElement.InvalidateVisual();
    ////    }
    ////    public static void Refresh(this UIElement uiElement, Action MyEmptyDelegate)
    ////    {
    ////        uiElement.Dispatcher.Invoke(MyEmptyDelegate, DispatcherPriority.Render);
    ////        uiElement.InvalidateVisual();
    ////    }
    ////}

    /// <summary>
    /// Interaction logic for PuzzlerCanvas.xaml
    /// </summary>
    public partial class PuzzlerCanvas : UserControl
    {
        public PuzzlerCanvas()
        {
            InitializeComponent();
        }

        int i = 0;
        protected override void OnRender(DrawingContext drawingContext)
        {
            //Rect testRect = new Rect(this.ActualWidth*0.25, this.ActualWidth*0.25, this.ActualWidth * 0.5, this.ActualHeight * 0.5);
            

            
            ////test code for fill rectangle
            //System.Windows.Media.Brush b2 = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,255,0,0));
            //System.Windows.Media.Pen p = new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 255, 0)), 3);
            //drawingContext.DrawRectangle(
            //        b2,
            //        p,
            //        testRect);

            //System.Windows.Point centerPoint = new System.Windows.Point(testRect.X + testRect.Width / 2, testRect.Y + testRect.Height / 2);
            //Typeface typeface = new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            //double size = Math.Sqrt(Math.Pow(testRect.Width, 2) + Math.Pow(testRect.Height, 2)) / 2;
            //FormattedText formattedText = new FormattedText("12345", System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, size, System.Windows.Media.Brushes.Black);

            //System.Windows.Point textLocation = new System.Windows.Point(centerPoint.X - formattedText.WidthIncludingTrailingWhitespace / 2, centerPoint.Y - formattedText.Height/2);
            //drawingContext.DrawText(formattedText, textLocation);

            
            //////test code for draw text
            ////string s = "9";
            ////System.Windows.Media.Brush b = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 255));
            ////double size = 26;// Math.Sqrt(Math.Pow(testRect.Width, 2) + Math.Pow(testRect.Height, 2)) / 2;
            ////Typeface tf = new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Italic, FontWeights.Normal, FontStretches.Normal);
            ////FormattedText formattedText = new FormattedText(s, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, tf , size, b);
            ////formattedText.TextAlignment = TextAlignment.Center;
            ////System.Windows.Point origin = new System.Windows.Point(testRect.X + testRect.Width / 2, testRect.Y + testRect.Height /2);

            ////drawingContext.DrawText(formattedText, origin);

            //return;
            if (MainWindow.PresentationLogicObject != null)
            {
                float x = (float)this.ActualWidth;
                float y = (float)this.ActualHeight;
                MainWindow.PresentationLogicObject.Draw(drawingContext, x, y);
                base.OnRender(drawingContext);
                i++;
            }
        }


        void presentationLogicObject_EventFillRectangle(object drawingContext, System.Drawing.Brush brush, float x, float y, float width, float height)
        {
            DrawingContext g = drawingContext as DrawingContext;
            if (g != null)
            {
                System.Windows.Media.Brush b = ConvertWinfromsObjects.ConvertBrush(brush as SolidBrush);
                //System.Windows.Media.Pen p = new System.Windows.Media.Pen(b, 2);
                g.DrawRectangle(
                    b,
                    /*p*/ null,
                    new Rect(x, y, width, height));
            }
        }
        void presentationLogicObject_EventDrawRectangle(object drawingContext, System.Drawing.Pen pen, float x, float y, float width, float height)
        {
            DrawingContext g = drawingContext as DrawingContext;
            if (g != null)
            {
                g.DrawRectangle(
                    null, 
                    ConvertWinfromsObjects.ConvertPen(pen), 
                    new Rect(x, y, width, height));
            }
        }
        void presentationLogicObject_EventDrawText(object drawingContext, string s, Font font, System.Drawing.Brush brush, RectangleF layoutRectangle, StringFormat format)
        {
            DrawingContext g = drawingContext as DrawingContext;
            if (g != null)
            {
                System.Windows.Point centerPoint = new System.Windows.Point(layoutRectangle.X + layoutRectangle.Width / 2, layoutRectangle.Y + layoutRectangle.Height / 2);
                Typeface typeface = new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                double size = Math.Sqrt(Math.Pow(layoutRectangle.Width, 2) + Math.Pow(layoutRectangle.Height, 2)) / 4;
                FormattedText formattedText = new FormattedText(
                    s, 
                    System.Globalization.CultureInfo.CurrentUICulture, 
                    FlowDirection.LeftToRight, 
                    typeface, 
                    size, 
                    ConvertWinfromsObjects.ConvertBrush(brush as System.Drawing.SolidBrush));
                System.Windows.Point textLocation = new System.Windows.Point(
                    centerPoint.X - formattedText.WidthIncludingTrailingWhitespace / 2, 
                    centerPoint.Y - formattedText.Height / 2);
                
                g.DrawText(formattedText, textLocation);
            }
        }
        void presentationLogicObject_EventDrawLine(object drawingContext, System.Drawing.Pen pen, float x1, float y1, float x2, float y2)
        {
            DrawingContext g = drawingContext as DrawingContext;
            if (g != null)
            {
                System.Windows.Media.Color newColor = System.Windows.Media.Color.FromArgb(pen.Color.A, pen.Color.R, pen.Color.G, pen.Color.B);
                g.DrawLine(new System.Windows.Media.Pen(new SolidColorBrush(newColor), 1), new System.Windows.Point(x1, y1), new System.Windows.Point(x2, y2));
            }
        }


        internal void HookEventsToPresentationLogicObject(PresentationLogicBase presentationLogicObject)
        {
            // hook up events
            MainWindow.PresentationLogicObject.EventDrawRectangle += presentationLogicObject_EventDrawRectangle;
            MainWindow.PresentationLogicObject.EventFillRectangle += presentationLogicObject_EventFillRectangle;
            MainWindow.PresentationLogicObject.EventDrawText += presentationLogicObject_EventDrawText;
            MainWindow.PresentationLogicObject.EventDrawLine += presentationLogicObject_EventDrawLine;
        }
    }
}
