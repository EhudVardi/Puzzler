using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Data;

namespace Logic
{
    public class LogicLayerGeneric<P,B>
    {
        public DataLayerGeneric<P> DataProxy;


        public FactoryGeneric<P, B> FactoryModule;
        public SolverGeneric<B> SolverModule;
        public TrackerGeneric<B> TrackerModule;

        
        protected void AttachSolverEvents()
        {
            this.SolverModule.StepCompleted += new EventHandler(SolverProxy_StepCompleted);
            this.SolverModule.SolveCompleted += new EventHandler(SolverProxy_SolveCompleted);

            this.FactoryModule.StepGenerated += new EventHandler(FactoryModule_StepGenerated);
        }

        public event EventHandler StepCompleted;
        public event EventHandler SolveCompleted;

        public event EventHandler LoadCompleted;

        public event EventHandler StepGenerated;


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

        protected virtual void OnLoadCompleted(EventArgs e)
        {
            if (LoadCompleted != null)
                LoadCompleted(this, e);
        }

        protected virtual void OnStepGenerated(EventArgs e)
        {
            if (StepGenerated != null)
                StepGenerated(this, e);
        }


        public virtual bool ReadFromFile(string fileName)
        {
            return LoadFromPuzzleObject(this.DataProxy.XMLToPuzzle(fileName));
        }


        public virtual bool ReadFromWeb(string url)
        {
            P puzzleFromWeb = this.DataProxy.WebToPuzzleObject(url);
            if (LoadFromPuzzleObject(puzzleFromWeb))
            {
                this.DataProxy.WritePuzzle(puzzleFromWeb, Configuration.FromWebFolder);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool ReadFromText(string text)
        {
            P puzzleFromText = this.DataProxy.TextToPuzzleObject(text);
            if (LoadFromPuzzleObject(puzzleFromText))
            {
                this.DataProxy.WritePuzzle(puzzleFromText, Configuration.FromTextFolder);
                return true;
            }
            else
            {
                return false;
            }
        }


        public virtual bool GenerateRandom()
        {
            P puzzleFromGenerator = this.FactoryModule.BoardToPuzzle(this.FactoryModule.GenerateRandom());
            if (LoadFromPuzzleObject(puzzleFromGenerator))
            {
                this.DataProxy.WritePuzzle(puzzleFromGenerator, Configuration.FromGeneratorFolder);
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual bool LoadFromPuzzleObject(P puzzle)
        {
            if (puzzle == null)
                return false;

            B board = this.FactoryModule.PuzzleToBoard(puzzle);

            this.TrackerModule = new TrackerGeneric<B>(this.FactoryModule.PuzzleToBoard(puzzle));

            this.SolverModule.Initialize();
            this.SolverModule.Board = board;
            this.SolverModule.Solve();

            this.OnLoadCompleted(EventArgs.Empty);

            return true;
        }


        public string GetPuzzleTypeDocumentsPath()
        {
            return DataProxy.GetPuzzleTypeDocumentsPath();
        }

        public string GetPuzzleName()
        {
            return DataProxy.GetPuzzleName();
        }


        void SolverProxy_SolveCompleted(object sender, EventArgs e)
        {
            OnSolveCompleted(EventArgs.Empty);
        }

        void SolverProxy_StepCompleted(object sender, EventArgs e)
        {
            OnStepCompleted(EventArgs.Empty);
        }

        void FactoryModule_StepGenerated(object sender, EventArgs e)
        {
            OnStepGenerated(EventArgs.Empty);

        }


        public bool? RequestSolveStatus()
        {
            if (this.SolverModule.Board != null)
                return this.SolverModule.IsSolved();
            else
                return null;
        }

        public bool? RequestValidStatus()
        {
            if (this.SolverModule.Board != null)
                return this.SolverModule.IsValid();
            else
                return null;
        }


        public B getTrackedBoard()
        {
            return this.SolverModule.Board;
        }

        public B getSolvedBoard()
        {
            return this.TrackerModule.Board;
        }
    }
}
