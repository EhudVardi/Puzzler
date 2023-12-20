using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Base
{
    public class CellBase
    {
        private int _row;

        public int Row
        {
            get { return _row; }
            set { _row = value; }
        }


        private int _column;

        public int Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public CellBase()
        {

        }

        public CellBase(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }


        public override string ToString()
        {
            return string.Format("({0},{1})", this.Row, this.Column);
        }
    }
}
