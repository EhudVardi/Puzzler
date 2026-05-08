using Common.Models.Base;

namespace Common.Models.Yakugo
{
    public class CellValueYakugo : CellValueBase<char?, GroupYakugo>
    {
        public bool IsInitialHint { get; set; }

        public CellValueYakugo() : base() { }

        public CellValueYakugo(int row, int column) : base(row, column) { }

        public override string ToString() =>
            $"({Row},{Column},{(Value.HasValue ? Value.Value.ToString() : "?")})";
    }
}
