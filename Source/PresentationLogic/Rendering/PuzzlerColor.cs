namespace PresentationLogic.Rendering
{
    public readonly struct PuzzlerColor
    {
        public byte A { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public PuzzlerColor(byte a, byte r, byte g, byte b) { A = a; R = r; G = g; B = b; }

        public static PuzzlerColor FromArgb(byte a, byte r, byte g, byte b) => new(a, r, g, b);
        public static PuzzlerColor FromArgb(byte a, PuzzlerColor c) => new(a, c.R, c.G, c.B);
        public PuzzlerColor WithAlpha(byte a) => new(a, R, G, B);

        public static readonly PuzzlerColor Black       = new(255,   0,   0,   0);
        public static readonly PuzzlerColor White       = new(255, 255, 255, 255);
        public static readonly PuzzlerColor Red         = new(255, 255,   0,   0);
        public static readonly PuzzlerColor Green       = new(255,   0, 128,   0);
        public static readonly PuzzlerColor Blue        = new(255,   0,   0, 255);
        public static readonly PuzzlerColor Navy        = new(255,   0,   0, 128);
        public static readonly PuzzlerColor Silver      = new(255, 192, 192, 192);
        public static readonly PuzzlerColor Snow        = new(255, 255, 250, 250);
        public static readonly PuzzlerColor Gray        = new(255, 128, 128, 128);
        public static readonly PuzzlerColor Yellow      = new(255, 255, 255,   0);
        public static readonly PuzzlerColor Transparent = new(  0,   0,   0,   0);
        public static readonly PuzzlerColor Wheat       = new(255, 245, 222, 179);
    }
}
