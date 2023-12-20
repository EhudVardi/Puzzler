using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Logic
{
    public class BooleanArrayChoicesMap
    {

        private bool[] _map;

        public bool[] Map
        {
            get { return _map; }
            set { _map = value; }
        }

        public int Count
        {
            get { return _map.Length; }
        }

        public BooleanArrayChoicesMap()
        {

        }

        public BooleanArrayChoicesMap(int n)
        {
            _map = new bool[n];
            for (int i = 0; i < n; i++)
            {
                _map[i] = true;
            }
        }

        public BooleanArrayChoicesMap(int n, bool value)
            : this(n)
        {
            for (int i = 0; i < n; i++)
            {
                _map[i] = false;
            }
        }

        public BooleanArrayChoicesMap(BooleanArrayChoicesMap numbers):this(numbers.Count)
        {
            for (int i = 0; i < numbers.Count; i++)
            {
                this.Map[i] = numbers.Map[i];
            }
        }

        public BooleanArrayChoicesMap AND(BooleanArrayChoicesMap nums)
        {
            BooleanArrayChoicesMap ans = null;
            if (this.Count == nums.Count)
            {
                ans = new BooleanArrayChoicesMap(this.Count);
                for (int i = 0; i < this.Count; i++)
                {
                    ans.Map[i] = this.Map[i] & nums.Map[i];
                }
            }
            return ans;
        }

        public BooleanArrayChoicesMap OR(BooleanArrayChoicesMap nums)
        {
            BooleanArrayChoicesMap ans = null;
            if (this.Count == nums.Count)
            {
                ans = new BooleanArrayChoicesMap(this.Count);
                for (int i = 0; i < this.Count; i++)
                {
                    ans.Map[i] = this.Map[i] | nums.Map[i];
                }
            }
            return ans;
        }

        public BooleanArrayChoicesMap NOT()
        {
            BooleanArrayChoicesMap ans = new BooleanArrayChoicesMap(this.Count);
            for (int i = 0; i < this.Count; i++)
            {
                ans.Map[i] = !Map[i];
            }
            return ans;
        }


        public int CountPositives()
        {
            int ans = 0;

            for (int i = 0; i < this.Count; i++)
            {
                if (this._map[i] == true)
                {
                    ans++;
                }
            }

            return ans;
        }

        public void SetSingle(int num)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (i == num)
                    this._map[i] = true;
                else
                    this._map[i] = false;
            }
        }


        public int GetNumber()
        {
            if (CountPositives() == 1)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this.Map[i] == true)
                        return i;
                }
            }
            return -1;
        }

        public bool Equals(BooleanArrayChoicesMap numbers)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this.Map[i] != numbers.Map[i])
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < this.Count; i++)
            {
                if (this._map[i])
                    s += "1";
                else
                    s += "0";
            }
            return s;
        }
    }
}
