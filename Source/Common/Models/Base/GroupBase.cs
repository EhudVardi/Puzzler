using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Base
{
    public abstract class GroupBase<C>
    {

        protected List<C> _cells;

        public virtual List<C> Cells
        {
            get { return _cells; }
            set { _cells = value; }
        }


        public virtual int Size { get {return this.Cells.Count; }}



        public virtual bool IsValid() { return false; }


        public virtual void Reset() { }
    }
}
