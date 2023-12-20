using System;
using System.Collections.Generic;
using System.Text;
using Common.Models.Base;

namespace Logic
{
    public class TrackerGeneric<B>
    {
        private B _board;

        public B Board
        {
            get { return _board; }
            set { _board = value; }
        }

	

        public TrackerGeneric()
        {

        }

        public TrackerGeneric(B board)
        {
            this.Board = board;
        }
    }
}
