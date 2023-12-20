using System;
using System.Collections.Generic;
using System.Text;
using Common.Logic;
using Common.Models.Base;

namespace Common.Models.Sudoku
{
    public class CellValueSudoku : CellValueBase<int?, GroupSudoku>
    {
        /*
        public override int? Value
        {
            get
            {
                int? number = this.ChoicesMap.GetNumber();
                return number != -1 ? number : null ;
            }
            set
            {
                if (value != null)
                    this.ChoicesMap.SetToNumber((int)value);
            }
        }
        */
        /*
        public override bool IsFixed
        {
            get
            {
                return this.ChoicesMap.IsSetToNumber();
            }
        }
        */
        /*
        private BinaryChoicesMap _choicesMap;

        public BinaryChoicesMap ChoicesMap
        {
            get { return _choicesMap; }
            set { _choicesMap = value; }
        }
        */

        public CellValueSudoku(int n)
        {
            //this.ChoicesMap = new BinaryChoicesMap(n);
            this.Groups = new List<GroupSudoku>();
        }

        public CellValueSudoku(int row, int column, int n):this(n)
        {
            this.Row = row;
            this.Column = column;
        }


        public void Reset(bool value)
        {
            //this.ChoicesMap.Reset(value);
            this.Value = null;
        }


        public override string ToString()
        {
            return string.Format("({0},{1})({2})({3})", this.Row, this.Column, this.Value /*this.ChoicesMap.ToString()*/, this.Value);
        }

        
    }
}
