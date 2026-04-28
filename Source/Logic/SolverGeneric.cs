using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Logic
{
    public class SolverGeneric<TBoard>
    {
        protected TBoard _board = default!;
        public TBoard Board
        {
            get { return _board; }
            set { _board = value; }
        }

        public SolverGeneric() { }

        public event EventHandler? StepCompleted;
        public event EventHandler? SolveCompleted;

        protected BackgroundWorker bg = null!;

        public virtual void Solve()
        {
            bg.RunWorkerAsync();
        }

        void bg_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            OnSolveCompleted(EventArgs.Empty);
        }
        void bg_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            OnStepCompleted(e);
        }
        void bg_DoWork(object? sender, DoWorkEventArgs e)
        {
            SolveBoard(e);
        }

        protected void ReportProgress(int precentageProgress, object? state)
        {
            bg.ReportProgress(precentageProgress, state);
        }

        public virtual void SolveBoard(DoWorkEventArgs e)
        {
            DateTime start = DateTime.Now;
            SolveInitiation();
            if (IsValid() == false)
                return;

            int precentageProgress = 0;
            while (!IsSolved())
            {
                if (bg.CancellationPending)
                    return;

                if (!DoCompleteStep())
                    break;

                ReportProgress(precentageProgress, null);
                precentageProgress++;
            }

            if (!IsSolved())
                BacktrackSolve();

            Console.WriteLine("total time = " + (DateTime.Now - start).TotalMilliseconds + "ms");
        }

        private bool BacktrackSolve()
        {
            while (!IsSolved() && DoCompleteStep()) { }

            if (!IsValid()) return false;
            if (IsSolved()) return true;

            foreach (Action tryBranch in GetBranches())
            {
                if (bg.CancellationPending) return false;
                object snapshot = TakeSnapshot();
                tryBranch();
                if (BacktrackSolve()) return true;
                RestoreSnapshot(snapshot);
            }
            return false;
        }

        protected virtual IEnumerable<Action> GetBranches() => Array.Empty<Action>();
        protected virtual object TakeSnapshot() =>
            throw new NotSupportedException($"{GetType().Name} does not support backtracking");
        protected virtual void RestoreSnapshot(object snapshot) =>
            throw new NotSupportedException($"{GetType().Name} does not support backtracking");

        public virtual void SolveInitiation() { }
        public virtual bool DoCompleteStep() { return false; }

        public virtual bool IsSolved() { return false; }
        public virtual bool IsValid() { return true; }

        public virtual void Reset() { }

        public virtual void SetCell(int row, int column, int num) { }

        protected virtual void OnStepCompleted(EventArgs e)
        {
            if (StepCompleted != null)
                StepCompleted(this, e);
        }
        protected virtual void OnSolveCompleted(EventArgs e)
        {
            if (SolveCompleted != null)
                SolveCompleted(this, e);
        }

        internal void Initialize()
        {
            if (bg != null)
            {
                if (bg.IsBusy)
                    bg.CancelAsync();
                while (bg.IsBusy) ;
                bg.DoWork             -= bg_DoWork;
                bg.ProgressChanged    -= bg_ProgressChanged;
                bg.RunWorkerCompleted -= bg_RunWorkerCompleted;
            }
            bg = new BackgroundWorker();
            bg.DoWork             += new DoWorkEventHandler(bg_DoWork);
            bg.ProgressChanged    += new ProgressChangedEventHandler(bg_ProgressChanged);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            bg.WorkerReportsProgress    = true;
            bg.WorkerSupportsCancellation = true;
        }
    }
}
