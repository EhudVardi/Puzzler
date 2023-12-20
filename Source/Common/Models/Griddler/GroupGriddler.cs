using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Common.Models.Base;

namespace Common.Models.Griddler
{
    public class GroupGriddlerRow : GroupGriddler
    {
        public GroupGriddlerRow():base() { }

        public GroupGriddlerRow(int count, List<int> numbers) : base(count, numbers) { }
    }

    public class GroupGriddlerColumn : GroupGriddler
    {
        public GroupGriddlerColumn() : base() { }

        public GroupGriddlerColumn(int count, List<int> numbers) : base(count, numbers) { }
    }

    public class GroupGriddler : GroupBase<CellValueGriddler>
    {

        private List<int> _numbers;

        public List<int> Numbers
        {
            get { return _numbers; }
            set { _numbers = value; }
        }

        

        public GroupGriddler()
        {
            _cells = new List<CellValueGriddler>();
            _numbers = new List<int>();
        }

        public GroupGriddler(int count)
            : this()
        {
            for (int i = 0; i < count; i++)
                _cells.Add(new CellValueGriddler());
        }


        public GroupGriddler(int count, List<int> numbers)
            : this(count)
        {
            this._numbers = numbers;
        }



        public void SetAll(bool? value)
        {
            foreach (CellValueGriddler cell in _cells)
            {
                cell.Value = value;
            }
        }



        public override string ToString()
        {
            string s = "";

            for (int i = 0; i < _cells.Count; i++)
            {
                s += _cells[i].ToString();
            }

            return s;
        }

    }
}
