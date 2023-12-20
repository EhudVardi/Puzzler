using System;
using System.Collections.Generic;
using System.Text;
using Common.Models.Base;

namespace Common.Models.Kakuru
{

    public class CellGroupHolderKakuru : CellGroupHolderBase
    {
        private GroupKakuru _rightGroup;

        public GroupKakuru RightGroup
        {
            get { return _rightGroup; }
            set { _rightGroup = value; }
        }


        private GroupKakuru _downGroup;

        public GroupKakuru DownGroup
        {
            get { return _downGroup; }
            set { _downGroup = value; }
        }

        public CellGroupHolderKakuru()
        {

        }

        public CellGroupHolderKakuru(int row, int column)
            : base(row, column)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
