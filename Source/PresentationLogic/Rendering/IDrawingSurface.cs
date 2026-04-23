namespace PresentationLogic.Rendering
{
    public interface IDrawingSurface
    {
        void FillRect(PuzzlerColor fill, float x, float y, float width, float height);
        void DrawRect(PuzzlerColor stroke, float thickness, float x, float y, float width, float height);
        void DrawText(string text, PuzzlerFont font, PuzzlerColor color, float x, float y, float width, float height);
        void DrawLine(PuzzlerColor color, float thickness, float x1, float y1, float x2, float y2);
        void DrawPolygon(PuzzlerColor stroke, float strokeThickness, PuzzlerColor fill, PuzzlerPoint[] points);
    }
}
