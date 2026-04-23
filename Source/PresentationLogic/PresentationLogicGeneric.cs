using System;
using System.Collections.Generic;
using Logic;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicGeneric<P,B> : PresentationLogicBase
    {
        public LogicLayerGeneric<P, B> LogicProxy;
        public string URL;

        public PresentationLogicGeneric() { }

        public override void Initialize()
        {
            this.LogicProxy.SolveCompleted += new EventHandler(Board_SolveCompleted);
            this.LogicProxy.StepCompleted  += new EventHandler(Board_StepCompleted);
            this.LogicProxy.LoadCompleted  += new EventHandler(LogicProxy_LoadCompleted);
            this.LogicProxy.StepGenerated  += new EventHandler(LogicProxy_StepGenerated);
        }

        public override Dictionary<string, List<string>> ReadFileList() { return this.LogicProxy.ReadFileList(); }
        public override bool ReadFromFile(string fileName) { return this.LogicProxy.ReadFromFile(fileName); }
        public override bool ReadFromWeb(string url)       { return this.LogicProxy.ReadFromWeb(this.URL); }
        public override bool ReadFromText(string text)     { return LogicProxy.ReadFromText(text); }

        public override bool GenerateRandom() { return LogicProxy.GenerateRandom(); }

        public override string GetPuzzleTypeDocumentsPath() { return LogicProxy.GetPuzzleTypeDocumentsPath(); }
        public virtual string GetPuzzleName() { return LogicProxy.GetPuzzleName(); }

        protected void Board_StepCompleted(object sender, EventArgs e)  { this.OnRequestRefresh(e); }
        protected void Board_SolveCompleted(object sender, EventArgs e) { this.OnRequestRefresh(EventArgs.Empty); }

        protected void LogicProxy_LoadCompleted(object sender, EventArgs e) { this.InitDisplay(); this.OnRequestRefresh(EventArgs.Empty); }
        protected void LogicProxy_StepGenerated(object sender, EventArgs e) { this.OnRequestRefresh(EventArgs.Empty); }

        public override bool? IsSolved() { return LogicProxy.RequestSolveStatus(); }
        public override bool? IsValid()  { return LogicProxy.RequestValidStatus(); }

        public override void Draw(IDrawingSurface surface, float width, float height)
        {
            base.Draw(surface, width, height);
            B trackerBoard = GetTrackerBoard();
            B solvedBoard  = GetSolvedBoard();
            if (trackerBoard != null && solvedBoard != null)
            {
                DrawBoard(trackerBoard, solvedBoard, width, height);
            }
            else
            {
                float fontSize = Math.Max(Math.Min(width, height), 1) / (float)this.GetPuzzleName().Length;
                DrawText(this.GetPuzzleName(), new PuzzlerFont("Arial", fontSize, false), PuzzlerColor.Green,
                    width / 3, height / 3, width / 3, height / 3);

                int rectCount = 50;
                for (int i = 0; i < rectCount / 2; i++)
                {
                    DrawRect(PuzzlerColor.Black, 1,
                        width / rectCount * i,
                        height / rectCount * i,
                        width - 2 * (width / rectCount * i),
                        height - 2 * (height / rectCount * i));
                }
            }
        }

        public virtual void DrawBoard(B trackerBoard, B solvedBoard, float width, float height) { }

        public virtual B GetTrackerBoard() { return this.LogicProxy.getSolvedBoard(); }
        public virtual B GetSolvedBoard()  { return this.LogicProxy.getTrackedBoard(); }
    }
}
