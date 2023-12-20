using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;
using Common.Models.Base;

namespace Logic
{
    public class FactoryGeneric<P,B>
    {

        public virtual P BoardToPuzzle(B board) { return CreatePuzzleObjectFromBoard(board); }

        public virtual B PuzzleToBoard(P puzzle) { return CreateBoardFromPuzzleObject(puzzle); }

        public virtual B GenerateRandom() { return default(B); }

        public event EventHandler StepGenerated;

        protected virtual B CreateBoardFromPuzzleObject(P puzzle)
        {
            return default(B);
        }

        protected virtual P CreatePuzzleObjectFromBoard(B board)
        {
            return default(P);
        }

        protected virtual void FireStepGenerated(object sender, EventArgs e)
        {
            if (this.StepGenerated != null)
            {
                StepGenerated(sender, e);
            }
        }

        protected virtual void OnStepGenerated() { }

        /*
        public virtual B CloneBoard(B board)
        {
            return PuzzleToBoard(BoardToPuzzle(board));
        }
        */
    }
}
