using System;

namespace Logic
{
    public class FactoryGeneric<TPuzzle, TBoard>
    {
        public virtual TPuzzle BoardToPuzzle(TBoard board) { return CreatePuzzleObjectFromBoard(board); }

        public virtual TBoard PuzzleToBoard(TPuzzle puzzle) { return CreateBoardFromPuzzleObject(puzzle); }

        public virtual TBoard GenerateRandom() { return default(TBoard); }

        public event EventHandler StepGenerated;

        protected virtual TBoard CreateBoardFromPuzzleObject(TPuzzle puzzle)
        {
            return default(TBoard);
        }

        protected virtual TPuzzle CreatePuzzleObjectFromBoard(TBoard board)
        {
            return default(TPuzzle);
        }

        protected virtual void FireStepGenerated(object sender, EventArgs e)
        {
            if (this.StepGenerated != null)
                StepGenerated(sender, e);
        }

        protected virtual void OnStepGenerated() { }
    }
}
