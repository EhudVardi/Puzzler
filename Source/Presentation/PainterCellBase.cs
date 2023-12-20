using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Common.Models.Base;

namespace Presentation
{

    public class PainterCellBase
    {
        private CellBase _cell;

        public CellBase Cell
        {
            get { return _cell; }
            set { _cell = value; }
        }



        protected static Brush bFixed = Brushes.LightGray;
        protected static Brush bNull = Brushes.LightBlue;
        protected static Brush bCorrect = Brushes.GreenYellow;
        protected static Brush bIncorrect = Brushes.Pink;
        protected static Brush bGroupHolder = Brushes.Gray;
        protected static Brush bText = Brushes.Black;
        protected static Brush bMark = Brushes.Black;


        protected static int margin = 1;

        protected static Font font;
        protected static StringFormat sf;

        static PainterCellBase()
        {
            font = new Font(FontFamily.GenericSerif, (int)12, GraphicsUnit.Pixel);
            sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
        }


        protected virtual RectangleF GetCellRectangle()
        {

            return default(RectangleF);
        }
    }

    public class PainterCellValue<T, G> : PainterCellBase
    {
        private CellValueBase<T, G> _trackedCell;
        public CellValueBase<T, G> TrackedCell
        {
            get { return _trackedCell; }
            set { _trackedCell = value; }
        }

        private CellValueBase<T, G> _solvedCell;
        public CellValueBase<T, G> SolvedCell
        {
            get { return _solvedCell; }
            set { _solvedCell = value; }
        }



        public virtual bool CompareTrackedValueToSolved()
        {
            return SolvedCell.Value.Equals(TrackedCell.Value);
        }

        
        public virtual void Draw(Graphics g, int rows, int columns, int crWidth, int crHeight)
        {
            //base.Draw(g, rows, columns);
        }
    }


    public class PainterCellGroupHolder : PainterCellBase
    {
        private CellGroupHolderBase _groupHolderCell;

        public CellGroupHolderBase GroupHolderCell
        {
            get { return _groupHolderCell; }
            set { _groupHolderCell = value; }
        }


        public virtual void Draw(Graphics g, int rows, int columns, int crWidth, int crHeight)
        {
            //base.Draw(g, rows, columns);
            
        }
    }


}