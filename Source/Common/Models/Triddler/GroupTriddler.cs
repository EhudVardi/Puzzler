using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Common.Models.Base;

namespace Common.Models.Triddler
{
    public class GroupTriddlerHorizontal : GroupTriddler
    {
        public GroupTriddlerHorizontal() : base() { }
        public GroupTriddlerHorizontal(int count, List<int> numbers) : base(count, numbers) { }
    }

    public class GroupTriddlerVerical : GroupTriddler
    {
        public GroupTriddlerVerical() : base() { }
        public GroupTriddlerVerical(int count, List<int> numbers) : base(count, numbers) { }
    }

    public class GroupTriddlerDiagonal : GroupTriddler
    {
        public GroupTriddlerDiagonal() : base() { }
        public GroupTriddlerDiagonal(int count, List<int> numbers) : base(count, numbers) { }
    }
    
    public class GroupTriddler : Common.Models.Griddler.GroupGriddler
    {
        private int myVar;

        public int MyProperty
        {
            get { return myVar; }
            set { myVar = value; }
        }
        
        public GroupTriddler() : base() { }
        public GroupTriddler(int count) : base(count) { }
        public GroupTriddler(int count, List<int> numbers) : base(count, numbers) { }
    }
}
