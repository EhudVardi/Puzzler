using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Common;

namespace PresentationLogic
{
    public class PresentationLogicBase
    {
        protected static Brush bFixed = Brushes.Silver;
        protected static Brush bNull = Brushes.Snow;
        protected static Brush bCorrect = Brushes.Navy;
        protected static Brush bIncorrect = Brushes.Red;
        protected static Brush bGroupHolder = Brushes.Gray;
        protected static Brush bText = Brushes.Black;
        protected static Brush bMark = Brushes.Black;

        protected static Font font;
        protected static StringFormat sf;
        protected static float margin = 1;

        protected DisplayType displayType;

        static PresentationLogicBase()
        {
            font = new Font(FontFamily.GenericSerif, (int)16, GraphicsUnit.Pixel);
            sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;     
        }

        public virtual void Initialize() { }

        public virtual bool ReadFromFile(string fileName) { return false; }
        public virtual bool ReadFromWeb(string url) { return false; }
        public virtual bool ReadFromText(string text) { return false; }

        public virtual bool GenerateRandom() { return false; }

        public virtual string GetPuzzleTypeDocumentsPath() { return null; }

        public virtual bool? IsSolved() { return false; }
        public virtual bool? IsValid() { return false; }

        public virtual void ShowSolution() { this.displayType = DisplayType.Solution; OnRequestRefresh(EventArgs.Empty);  }
        public virtual void ShowHints() { this.displayType = DisplayType.Hint; OnRequestRefresh(EventArgs.Empty); }
        public virtual void ShowBoard() { this.displayType = DisplayType.Board; OnRequestRefresh(EventArgs.Empty); }

        public virtual void InitDisplay() { }

        public virtual void Draw(object drawingContext, float width, float height) { }
        
        //TODO: replace each method's params with those: MouseButtons button, int delta, float x, float y, float sizeX, float sizeY
        public virtual void InputMouse(MouseEventArgs e, Size s) { }
        public virtual void InputMouseDown(MouseEventArgs e, Size s) { }
        public virtual void InputMouseMove(MouseEventArgs e, Size s) { }
        public virtual void InputMouseUp(MouseEventArgs e, Size s) { }
        public virtual void InputMouseWheel(MouseEventArgs e, Size size) { }

        //TODO: replace each method's params with those: int keyValue
        public virtual void InputKey(KeyEventArgs e) { }

        public event EventHandler Refresh;
        public event DrawRectangle EventDrawRectangle;
        public event FillRectangle EventFillRectangle;
        public event DrawText EventDrawText;
        public event DrawLine EventDrawLine;

        public delegate void DrawRectangle(object drawingContext, Pen pen, float x, float y, float width, float height);
        public delegate void FillRectangle(object drawingContext, Brush brush, float x, float y, float width, float height);
        public delegate void DrawText(object drawingContext, string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format);
        //TODO: add parameter "Thickness"
        public delegate void DrawLine(object drawingContext, Pen pen, float x1, float y1, float x2, float y2);

        protected virtual void OnRequestRefresh(EventArgs e)
        {
            if (this.Refresh != null)
                this.Refresh(null, e);
        }
        protected virtual void OnRequestDrawRectangle(object drawingContext, Pen pen, float x, float y, float width, float height)
        {
            if (this.EventDrawRectangle != null)
                this.EventDrawRectangle(drawingContext, pen, x, y, width, height);
        }
        protected virtual void OnRequestFillRectangle(object drawingContext, Brush brush, float x, float y, float width, float height)
        {
            if (this.EventFillRectangle != null)
                this.EventFillRectangle(drawingContext, brush, x, y, width, height);
        }
        protected virtual void OnRequestDrawText(object drawingContext, string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
        {

            if (this.EventDrawText != null)
                this.EventDrawText(drawingContext, s, font, brush, layoutRectangle, format);
        }
        protected virtual void OnRequestDrawLine(object drawingContext, Pen pen, float x1, float y1, float x2, float y2)
        {
            if (this.EventDrawLine != null)
                this.EventDrawLine(drawingContext, pen, x1, y1, x2, y2);
        }
    }

    public enum DisplayType
    {
        Board, Hint, Solution
    }
}
