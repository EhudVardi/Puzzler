using System;
using System.Collections.Generic;
using System.Text;
using Common.Models.Base;

namespace Common.Models.Triddler
{
    public class CellValueTriddler : Common.Models.Griddler.CellValueGriddler
    {
        private bool _side;

        public bool Side
        {
            get { return _side; }
            set { _side = value; }
        }
        public bool IsLeft { get { return _side == false; } }
        public bool IsRight { get { return _side == true; } }

        public CellValueTriddler(int row, int column)
            : base(row, column) { }

        public CellValueTriddler(int row, int column, bool side)
            : this(row, column) 
        {
            this.Side = side;
        }

        public override string ToString()
        {
            return string.Format("({0},{1},{2},{3})", this.Row, this.Column, (this.Side == true ? "R" : "L"), (this.Value == null ? "?" : this.Value == true ? "1" : "0"));
        }
    }
}
