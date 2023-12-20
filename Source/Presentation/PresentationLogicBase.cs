using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Common;

namespace Presentation
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

        public event EventHandler Refresh;

        
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

        public virtual void Draw(PaintEventArgs e) { }



        public virtual void InputMouse(MouseEventArgs e, Size s) { }
        public virtual void InputMouseDown(MouseEventArgs e, Size s) { }
        public virtual void InputMouseMove(MouseEventArgs e, Size s) { }
        public virtual void InputMouseUp(MouseEventArgs e, Size s) { }
        public virtual void InputMouseWheel(MouseEventArgs e, Size size) { }


        public virtual void InputKey(KeyEventArgs e) { }


        protected virtual void OnRequestRefresh(EventArgs e)
        {
            if (this.Refresh != null)
                this.Refresh(null, e);

        }




    }

    public enum DisplayType
    {
        Board, Hint, Solution
    }
}
