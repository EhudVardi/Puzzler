using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Common.Logic
{
    public class BinaryChoicesMap
    {
        #region properties and members

        private BitArray _map;

        public BitArray Map
        {
            get { return _map; }
            set { _map = value; }
        }

        public int Count
        {
            get { return _map.Count; }
        }

        public int Ones
        {
            get 
            {
                int ans = 0;

                for (int i = 0; i < this.Count; i++)
                    if (this._map[i] == true)
                        ans++;

                return ans;
            }
        }

        public int Zeros
        {
            get { return this.Count - this.Ones; }
        }


        #endregion

        #region constructors

        public BinaryChoicesMap()
        {

        }

        public BinaryChoicesMap(int n)
        {
            _map = new BitArray(n, true);
        }

        public BinaryChoicesMap(int n, bool value)
            : this(n)
        {
            _map.SetAll(value);
        }

        public BinaryChoicesMap(BinaryChoicesMap numbers)
            : this(numbers.Count)
        {
            this.Map = new BitArray(numbers.Map);
        }

        #endregion

        #region logical operations

        public static BinaryChoicesMap AND(BinaryChoicesMap nums1, BinaryChoicesMap nums2)
        {
            BinaryChoicesMap ans = null;
            if (nums1.Count == nums2.Count)
            {
                ans = new BinaryChoicesMap(nums1);
                ans.Map.And(nums2.Map);
            }
            return ans;
        }
        public BinaryChoicesMap AND(BinaryChoicesMap nums2)
        {
            if (this.Count == nums2.Count)
            {
                this.Map.And(nums2.Map);
            }
            return this;
        }

        public static BinaryChoicesMap OR(BinaryChoicesMap nums1, BinaryChoicesMap nums2)
        {
            BinaryChoicesMap ans = null;
            if (nums1.Count == nums2.Count)
            {
                ans = new BinaryChoicesMap(nums1);
                ans.Map.Or(nums2.Map);
            }
            return ans;
        }
        public BinaryChoicesMap OR(BinaryChoicesMap nums2)
        {
            if (this.Count == nums2.Count)
            {
                this.Map.Or(nums2.Map);
            }
            return this;
        }

        public static BinaryChoicesMap NOT(BinaryChoicesMap nums)
        {
            BinaryChoicesMap ans = new BinaryChoicesMap(nums);
            ans.Map.Not();
            return ans;
        }
        public BinaryChoicesMap NOT()
        {
            this.Map.Not();
            return this;
        }

        public static BinaryChoicesMap XOR(BinaryChoicesMap nums1, BinaryChoicesMap nums2)
        {
            BinaryChoicesMap ans = null;
            if (nums1.Count == nums2.Count)
            {
                ans = new BinaryChoicesMap(nums1);
                ans.Map.Xor(nums2.Map);
            }
            return ans;
        }
        public BinaryChoicesMap XOR(BinaryChoicesMap nums2)
        {
            if (this.Count == nums2.Count)
            {
                this.Map.Xor(nums2.Map);
            }
            return this;
        }

        #endregion

        #region boolean operation

        public void SetSingleBit(int num, bool value)
        {
            this.Map.Set(num, value);
        }
        public bool GetSingleBit(int num)
        {
            return this.Map[num];
        }
       
        public bool BinaryEqualTo(BinaryChoicesMap numbers)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if ((this.Map[i] ^ numbers.Map[i]))
                    return false;
            }
            return true;
        }

        #endregion

        #region numerical operations

        public void SetToNumber(int num)
        {
            this.Map.SetAll(false);
            this.Map.Set(num, true);
        }

        public int GetNumber()
        {
            if (this.IsSetToNumber())
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this.Map[i] == true)
                        return i;
                }
            }
            return -1;
        }

        public bool IsSetToNumber()
        {
            return this.Ones == 1 ? true : false;
        }

        public void Reset(bool value)
        {
            this.Map.SetAll(value);
        }

        #endregion

        #region overrided

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

        #endregion

    }
}
