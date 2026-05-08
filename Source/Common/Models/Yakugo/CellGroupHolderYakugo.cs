using System.Collections.Generic;
using Common.Models.Base;

namespace Common.Models.Yakugo
{
    public class CellGroupHolderYakugo : CellGroupHolderBase
    {
        public List<GroupYakugo> Clues { get; } = new List<GroupYakugo>();

        public CellGroupHolderYakugo() : base() { }

        public CellGroupHolderYakugo(int row, int column) : base(row, column) { }
    }
}
