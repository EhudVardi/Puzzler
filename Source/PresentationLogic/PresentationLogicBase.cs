using System;
using System.Collections.Generic;
using Common;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicBase
    {
        protected static readonly PuzzlerColor bFixed       = PuzzlerColor.Silver;
        protected static readonly PuzzlerColor bNull        = PuzzlerColor.Snow;
        protected static readonly PuzzlerColor bCorrect     = PuzzlerColor.Navy;
        protected static readonly PuzzlerColor bIncorrect   = PuzzlerColor.Red;
        protected static readonly PuzzlerColor bGroupHolder = PuzzlerColor.Gray;
        protected static readonly PuzzlerColor bText        = PuzzlerColor.Black;
        protected static readonly PuzzlerColor bMark        = PuzzlerColor.Black;

        protected static readonly PuzzlerFont font     = PuzzlerFont.DefaultSerif;
        protected static readonly PuzzlerFont fontBold = PuzzlerFont.DefaultSerifBold;
        protected static float margin = 2;

        public PuzzlerOptions Options { get; set; } = PuzzlerOptions.CreateDefault();

        protected DisplayType displayType;
        private IDrawingSurface _surface;

        public virtual void Initialize() { }

        public virtual Dictionary<string, List<string>> ReadFileList() { return null; }
        public virtual bool ReadFromFile(string fileName) { return false; }
        public virtual bool ReadFromWeb(string url) { return false; }
        public virtual bool ReadFromText(string text) { return false; }

        public virtual bool GenerateRandom() { return false; }

        public virtual string GetPuzzleTypeDocumentsPath() { return null; }

        public virtual bool? IsSolved() { return false; }
        public virtual bool? IsValid() { return false; }

        public virtual void ShowSolution() { this.displayType = DisplayType.Solution; OnRequestRefresh(EventArgs.Empty); }
        public virtual void ShowHints()    { this.displayType = DisplayType.Hint;     OnRequestRefresh(EventArgs.Empty); }
        public virtual void ShowBoard()    { this.displayType = DisplayType.Board;    OnRequestRefresh(EventArgs.Empty); }

        public virtual void InitDisplay() { }

        public virtual (int Width, int Height) GetPrefferedSize() { return (500, 500); }

        public virtual void Draw(IDrawingSurface surface, float width, float height)
        {
            _surface = surface;
        }

        public virtual void HandlePointer(PointerEvent e, float sizeX, float sizeY) { }
        public virtual void HandlePointerDown(PointerEvent e, float sizeX, float sizeY) { }
        public virtual void HandlePointerMove(PointerEvent e, float sizeX, float sizeY) { }
        public virtual void HandlePointerUp(PointerEvent e, float sizeX, float sizeY) { }
        public virtual void HandlePointerWheel(PointerEvent e, float sizeX, float sizeY) { }
        public virtual void HandleKey(KeyEvent e) { }

        public event EventHandler Refresh;

        protected virtual void OnRequestRefresh(EventArgs e)
        {
            Refresh?.Invoke(null, e);
        }

        protected void FillRect(PuzzlerColor fill, float x, float y, float w, float h)
            => _surface?.FillRect(fill, x, y, w, h);

        protected void DrawRect(PuzzlerColor stroke, float thickness, float x, float y, float w, float h)
            => _surface?.DrawRect(stroke, thickness, x, y, w, h);

        protected void DrawText(string text, PuzzlerFont f, PuzzlerColor color, float x, float y, float w, float h)
            => _surface?.DrawText(text, f, color, x, y, w, h);

        protected void DrawLine(PuzzlerColor color, float thickness, float x1, float y1, float x2, float y2)
            => _surface?.DrawLine(color, thickness, x1, y1, x2, y2);

        protected void DrawPolygon(PuzzlerColor stroke, float strokeThickness, PuzzlerColor fill, PuzzlerPoint[] points)
            => _surface?.DrawPolygon(stroke, strokeThickness, fill, points);
    }

    public enum DisplayType
    {
        Board, Hint, Solution
    }
}
