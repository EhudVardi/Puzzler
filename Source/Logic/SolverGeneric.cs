using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Logic
{
    public class SolverGeneric<B>
    {

        protected B _board;

        public B Board
        {
            get { return _board; }
            set { _board = value; }
        }


        public SolverGeneric()
        {

        }
        

        public event EventHandler StepCompleted;
        public event EventHandler SolveCompleted;


        protected BackgroundWorker bg;




        public virtual void Solve()
        {
            bg.RunWorkerAsync();
        }


        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnSolveCompleted(EventArgs.Empty);
        }

        void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnStepCompleted(EventArgs.Empty);
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            SolveBoard(e);
        }


        public virtual void SolveBoard(DoWorkEventArgs e)
        {
            SolveInitiation();

            int roundCount = 0;

            while (!IsSolved() && IsValid())
            {
                if (bg.CancellationPending)
                {
                    return;
                }

                DoCompleteStep();

                bg.ReportProgress(0, roundCount);

                roundCount++;
            }

        }

        public virtual void SolveInitiation() { }

        public virtual bool DoCompleteStep() { return false; }



        public virtual bool IsSolved() { return false; }

        public virtual bool IsValid() { return true; }


        public virtual void Reset() { }


        public virtual void SetCell(int row, int column, int num)
        {

        }


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
                //while (bg.IsBusy) ;

                bg.DoWork -= bg_DoWork;
                bg.ProgressChanged -= bg_ProgressChanged;
                bg.RunWorkerCompleted -= bg_RunWorkerCompleted;
            
            }
            bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            bg.WorkerReportsProgress = true;
            bg.WorkerSupportsCancellation = true;
        }
    }
}
