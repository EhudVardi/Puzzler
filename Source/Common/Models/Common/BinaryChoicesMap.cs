using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Common.Logic
{
    public class BinaryChoicesMap
    {
        #region properties and members

        private BitArray _map = null!;
        private int _onesCount;

        public BitArray Map
        {
            get { return _map; }
            set
            {
                _map = value;
                _onesCount = ComputeOnes();
            }
        }

        public int Count
        {
            get { return _map.Count; }
        }

        public int Ones => _onesCount;

        public int Zeros => Count - _onesCount;

        #endregion

        #region constructors

        public BinaryChoicesMap()
        {
            _onesCount = 0;
        }

        public BinaryChoicesMap(int n)
        {
            _map = new BitArray(n, true);
            _onesCount = n;
        }

        public BinaryChoicesMap(int n, bool value)
            : this(n)
        {
            _map.SetAll(value);
            _onesCount = value ? n : 0;
        }

        public BinaryChoicesMap(BinaryChoicesMap numbers)
            : this(numbers.Count)
        {
            _map = new BitArray(numbers.Map);
            _onesCount = numbers._onesCount;
        }

        #endregion

        #region logical operations

        public static BinaryChoicesMap? AND(BinaryChoicesMap nums1, BinaryChoicesMap nums2)
        {
            BinaryChoicesMap? ans = null;
            if (nums1.Count == nums2.Count)
            {
                ans = new BinaryChoicesMap(nums1);
                ans.AND(nums2);
            }
            return ans;
        }
        public BinaryChoicesMap AND(BinaryChoicesMap nums2)
        {
            if (this.Count == nums2.Count)
            {
                this._map.And(nums2._map);
                _onesCount = ComputeOnes();
            }
            return this;
        }

        public static BinaryChoicesMap? OR(BinaryChoicesMap nums1, BinaryChoicesMap nums2)
        {
            BinaryChoicesMap? ans = null;
            if (nums1.Count == nums2.Count)
            {
                ans = new BinaryChoicesMap(nums1);
                ans.OR(nums2);
            }
            return ans;
        }
        public BinaryChoicesMap OR(BinaryChoicesMap nums2)
        {
            if (this.Count == nums2.Count)
            {
                this._map.Or(nums2._map);
                _onesCount = ComputeOnes();
            }
            return this;
        }

        public static BinaryChoicesMap NOT(BinaryChoicesMap nums)
        {
            BinaryChoicesMap ans = new BinaryChoicesMap(nums);
            ans.NOT();
            return ans;
        }
        public BinaryChoicesMap NOT()
        {
            this._map.Not();
            _onesCount = Count - _onesCount;
            return this;
        }

        public static BinaryChoicesMap? XOR(BinaryChoicesMap nums1, BinaryChoicesMap nums2)
        {
            BinaryChoicesMap? ans = null;
            if (nums1.Count == nums2.Count)
            {
                ans = new BinaryChoicesMap(nums1);
                ans.XOR(nums2);
            }
            return ans;
        }
        public BinaryChoicesMap XOR(BinaryChoicesMap nums2)
        {
            if (this.Count == nums2.Count)
            {
                this._map.Xor(nums2._map);
                _onesCount = ComputeOnes();
            }
            return this;
        }

        #endregion

        #region boolean operation

        public void SetSingleBit(int num, bool value)
        {
            bool current = this._map[num];
            if (current != value)
            {
                this._map.Set(num, value);
                _onesCount += value ? 1 : -1;
            }
        }
        public bool GetSingleBit(int num)
        {
            return this._map[num];
        }

        public bool BinaryEqualTo(BinaryChoicesMap numbers)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if ((this._map[i] ^ numbers._map[i]))
                    return false;
            }
            return true;
        }

        #endregion

        #region numerical operations

        public void SetToNumber(int num)
        {
            this._map.SetAll(false);
            this._map.Set(num, true);
            _onesCount = 1;
        }

        public int GetNumber()
        {
            if (this.IsSetToNumber())
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this._map[i] == true)
                        return i;
                }
            }
            return -1;
        }

        public bool IsSetToNumber() => _onesCount == 1;

        public void Reset(bool value)
        {
            this._map.SetAll(value);
            _onesCount = value ? Count : 0;
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

        #region private helpers

        private int ComputeOnes()
        {
            int count = 0;
            for (int i = 0; i < _map.Count; i++)
                if (_map[i]) count++;
            return count;
        }

        #endregion
    }
}
