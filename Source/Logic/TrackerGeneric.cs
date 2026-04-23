namespace Logic
{
    public class TrackerGeneric<TBoard>
    {
        private TBoard _board;
        public TBoard Board
        {
            get { return _board; }
            set { _board = value; }
        }

        public TrackerGeneric() { }

        public TrackerGeneric(TBoard board)
        {
            this.Board = board;
        }
    }
}
