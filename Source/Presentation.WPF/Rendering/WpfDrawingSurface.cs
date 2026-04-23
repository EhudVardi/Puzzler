using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using PresentationLogic.Rendering;

namespace Presentation.WPF
{
    internal sealed class WpfDrawingSurface : IDrawingSurface
    {
        private readonly DrawingContext _dc;

        public WpfDrawingSurface(DrawingContext dc) { _dc = dc; }

        public void FillRect(PuzzlerColor fill, float x, float y, float width, float height)
        {
            _dc.DrawRectangle(ToBrush(fill), null, new Rect(x, y, width, height));
        }

        public void DrawRect(PuzzlerColor stroke, float thickness, float x, float y, float width, float height)
        {
            _dc.DrawRectangle(null, ToPen(stroke, thickness), new Rect(x, y, width, height));
        }

        public void DrawText(string text, PuzzlerFont font, PuzzlerColor color, float x, float y, float width, float height)
        {
            var typeface = new Typeface(
                new FontFamily(font.Family),
                FontStyles.Normal,
                font.Bold ? FontWeights.Bold : FontWeights.Normal,
                FontStretches.Normal);
            double size = Math.Sqrt((double)width * width + (double)height * height) / 2;
            size = Math.Max(size, 1);
            var ft = new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                size,
                ToBrush(color),
                96.0);
            ft.TextAlignment = TextAlignment.Center;
            _dc.DrawText(ft, new Point(x + width / 2, y + (height - ft.Height) / 2));
        }

        public void DrawLine(PuzzlerColor color, float thickness, float x1, float y1, float x2, float y2)
        {
            _dc.DrawLine(ToPen(color, thickness), new Point(x1, y1), new Point(x2, y2));
        }

        public void DrawPolygon(PuzzlerColor stroke, float strokeThickness, PuzzlerColor fill, PuzzlerPoint[] points)
        {
            if (points == null || points.Length < 3) return;

            var segments = new LineSegment[points.Length - 1];
            for (int i = 1; i < points.Length; i++)
                segments[i - 1] = new LineSegment(new Point(points[i].X, points[i].Y), true);

            var figure = new PathFigure(new Point(points[0].X, points[0].Y), segments, true);
            var geo    = new PathGeometry(new[] { figure });

            _dc.DrawGeometry(
                fill.A > 0 ? ToBrush(fill) : null,
                strokeThickness > 0 ? ToPen(stroke, strokeThickness) : null,
                geo);
        }

        private static SolidColorBrush ToBrush(PuzzlerColor c)
            => new(Color.FromArgb(c.A, c.R, c.G, c.B));

        private static Pen ToPen(PuzzlerColor c, float thickness)
            => new(ToBrush(c), thickness);
    }
}
