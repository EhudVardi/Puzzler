using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.DataModels;
using Logic;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicGeneric<TPuzzle, TBoard> : PresentationLogicBase where TPuzzle : PuzzleBase
    {
        public LogicLayerGeneric<TPuzzle, TBoard> LogicProxy { get; set; } = null!;
        public string URL { get; set; } = null!;

        public PresentationLogicGeneric() { }

        public override void Initialize()
        {
            this.LogicProxy.DataProxy.Options = this.Options;
            this.LogicProxy.SolveCompleted += new EventHandler(Board_SolveCompleted);
            this.LogicProxy.StepCompleted  += new EventHandler(Board_StepCompleted);
            this.LogicProxy.LoadCompleted  += new EventHandler(LogicProxy_LoadCompleted);
            this.LogicProxy.StepGenerated  += new EventHandler(LogicProxy_StepGenerated);
        }

        public override bool IsSolving => LogicProxy.IsSolving;

        public override List<string> ReadFileList() { return this.LogicProxy.ReadFileList(); }
        public override string GetPuzzleSizeLabel(string filePath) => LogicProxy.GetPuzzleSizeLabel(filePath);
        public override PuzzleBase GetPuzzleMetadata(string filePath) => LogicProxy.GetPuzzleMetadata(filePath);
        public override Task<bool> ReadFromFile(string fileName) => this.LogicProxy.ReadFromFile(fileName);
        public override Task<bool> ReadFromWeb(string url)       => this.LogicProxy.ReadFromWeb(this.URL);
        public override Task<bool> ReadFromText(string text)     => LogicProxy.ReadFromText(text);

        public override Task<bool> GenerateRandom() => LogicProxy.GenerateRandom();

        public override string? GetPuzzleTypeDocumentsPath() { return LogicProxy.GetPuzzleTypeDocumentsPath(); }
        public virtual string GetPuzzleName() { return LogicProxy.GetPuzzleName(); }

        protected void Board_StepCompleted(object? sender, EventArgs e)  { this.OnRequestRefresh(e); }
        protected void Board_SolveCompleted(object? sender, EventArgs e) { this.OnRequestRefresh(EventArgs.Empty); }

        protected void LogicProxy_LoadCompleted(object? sender, EventArgs e) { this.InitDisplay(); this.OnRequestRefresh(EventArgs.Empty); }
        protected void LogicProxy_StepGenerated(object? sender, EventArgs e) { this.OnRequestRefresh(EventArgs.Empty); }

        public override bool? IsSolved() { return LogicProxy.RequestSolveStatus(); }
        public override bool? IsValid()  { return LogicProxy.RequestValidStatus(); }

        public override void Draw(IDrawingSurface surface, float width, float height)
        {
            base.Draw(surface, width, height);
            TBoard? trackerBoard = GetTrackerBoard();
            TBoard? solvedBoard  = GetSolvedBoard();
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

        public virtual void DrawBoard(TBoard trackerBoard, TBoard solvedBoard, float width, float height) { }

        public virtual TBoard? GetTrackerBoard() { return this.LogicProxy.getSolvedBoard(); }
        public virtual TBoard? GetSolvedBoard()  { return this.LogicProxy.getTrackedBoard(); }
    }
}
