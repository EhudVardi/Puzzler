using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Base
{
    public class CellValueBase<T, G> : CellBase
    {
        protected T _value;

        public virtual T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        protected List<G> _groups;

        public virtual List<G> Groups
        {
            get { return _groups; }
            set { _groups = value; }

        }


        public virtual bool IsFixed
        {
            get { return Value != null ? true : false; }
        }

        public CellValueBase()
        {
            this.Groups = new List<G>();
        }

        public CellValueBase(int row, int column):base(row,column)
        {
            this.Groups = new List<G>();
        }

    }
}
