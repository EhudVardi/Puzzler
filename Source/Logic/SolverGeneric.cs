using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

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

        private CancellationTokenSource? _cts;
        private Task?                     _workTask;
        protected CancellationToken       _ct;
        protected IProgress<int>?         _progress;

        public bool IsSolving => _workTask is { IsCompleted: false };

        public virtual void Solve()
        {
            _cts      = new CancellationTokenSource();
            _ct       = _cts.Token;
            _progress = new Progress<int>(pct => OnStepCompleted(new ProgressChangedEventArgs(pct, null)));
            var scheduler = SynchronizationContext.Current != null
                ? TaskScheduler.FromCurrentSynchronizationContext()
                : TaskScheduler.Default;
            _workTask = Task.Run(SolveBoard, _ct)
                            .ContinueWith(_ => OnSolveCompleted(EventArgs.Empty),
                                          CancellationToken.None,
                                          TaskContinuationOptions.None,
                                          scheduler);
        }

        internal async Task Initialize()
        {
            if (_cts != null && _workTask != null)
            {
                _cts.Cancel();
                try { await Task.WhenAny(_workTask); } catch { }
                _cts.Dispose();
                _cts      = null;
                _workTask = null;
            }
        }

        protected void ReportProgress(int pct) => _progress?.Report(pct);

        public virtual void SolveBoard()
        {
            DateTime start = DateTime.Now;
            SolveInitiation();
            if (!IsValid()) return;

            int pct = 0;
            while (!IsSolved())
            {
                if (_ct.IsCancellationRequested) return;
                if (!DoCompleteStep()) break;
                ReportProgress(pct++);
            }

            if (!IsSolved()) BacktrackSolve();
            Console.WriteLine("total time = " + (DateTime.Now - start).TotalMilliseconds + "ms");
        }

        private bool BacktrackSolve()
        {
            while (!IsSolved() && DoCompleteStep()) { }

            if (!IsValid()) return false;
            if (IsSolved()) return true;

            foreach (Action tryBranch in GetBranches())
            {
                if (_ct.IsCancellationRequested) return false;
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
    }
}
