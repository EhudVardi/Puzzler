using System;
using System.Collections.Generic;
using System.Text;
using Common.Models.Base;

namespace Common.Models.Griddler
{
    public class CellValueGriddler : CellValueBase<bool?, GroupGriddler>
    {

        public CellValueGriddler()
        {
            _value = null;
        }


        public CellValueGriddler(int row, int column) : this()
        {
            this.Row = row;
            this.Column = column;
        }



        public CellValueGriddler(bool? value)
        {
            _value = value;
        }


        
        public static bool? AND(bool? b1, bool? b2)
        {
            if (b1 == null || b2 == null)
                return null;
            else if (b1 == false || b2 == false)
                return false;
            else
                return true;
        }

        public static bool? OR(bool? b1, bool? b2)
        {
            if (b1 == null || b2 == null)
                return null;
            else if (b1 == false && b2 == false)
                return false;
            else
                return true;
        }

        public static bool? XOR(bool? b1, bool? b2)
        {
            if (b1 == null || b2 == null)
                return null;
            else if ((b1 == true && b2 == false) || (b1 == false && b2 == true))
                return true;
            else
                return false;
        }

        public static bool? NOT(bool? b1)
        {
            if (b1 == null)
                return null;
            else
                return !b1;
        }





        public override string ToString()
        {
            return this.Value == null ? "?" : this.Value == true ? "1" : "0";
        }
    }
}
