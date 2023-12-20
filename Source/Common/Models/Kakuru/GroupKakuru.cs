using System;
using System.Collections.Generic;
using System.Text;
using Common.Logic;
using Common.Models.Base;

namespace Common.Models.Kakuru
{
    public class GroupKakuruRow : GroupKakuru
    {

    }

    public class GroupKakuruColumn : GroupKakuru
    {

    }


    public class GroupKakuru : GroupBase<CellValueKakuru>
    {
        
        private int _sum;

        public int Sum
        {
            get { return _sum; }
            set { _sum = value; }
        }


        private bool _rightDown;

        public bool RightDown
        {
            get { return _rightDown; }
            set { _rightDown = value; }
        }


    }
}
