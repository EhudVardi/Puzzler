using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Common.Models.Base;

namespace Common.Models.Griddler
{
    public class BoardGriddler : BoardGeneric<GroupGriddler, CellValueGriddler, CellGroupHolderGriddler>
    {

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < _cellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < _cellsMatrix.GetLength(1); j++)
                {
                    str += _cellsMatrix[i, j].ToString();
                }
                str += "\n";
            }
            return str;
        }

    }
}