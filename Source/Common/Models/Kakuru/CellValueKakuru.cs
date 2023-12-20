using System;
using System.Collections.Generic;
using System.Text;
using Common.Logic;
using Common.Models.Base;

namespace Common.Models.Kakuru
{
    public class CellValueKakuru:CellValueBase<int?, GroupKakuru>
    {
        public CellValueKakuru()
        {

        }

        public CellValueKakuru(int row, int column) : base(row, column) 
        {

        }

        public CellValueKakuru(CellValueKakuru fillCell)
        {
            this._value = fillCell.Value;
            this.Row = fillCell.Row;
            this.Column = fillCell.Column;
            this.Groups = fillCell.Groups;
        }

        
    }


}
