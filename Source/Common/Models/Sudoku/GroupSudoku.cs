using System;
using System.Collections.Generic;
using System.Text;
using Common.Logic;
using Common.Models.Base;

namespace Common.Models.Sudoku
{
    public class GroupSudoku : GroupBase<CellValueSudoku>
    {

        public GroupSudoku()
        {
            this._cells = new List<CellValueSudoku>();
        }

        public GroupSudoku(List<CellValueSudoku> cells) :this()
        {
            this._cells = cells;
            foreach (CellValueSudoku cell in cells) 
                cell.Groups.Add(this);

        }



        public override string ToString()
        {
            string s = "{";
            for (int i = 0; i < _cells.Count; i++)
            {
                s += Cells[i].ToString() + ", ";
            }
            s = s.Substring(0, s.Length - 2) + "}";

            return s;
        }



        
    }
}
