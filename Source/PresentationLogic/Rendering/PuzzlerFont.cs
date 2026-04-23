namespace PresentationLogic.Rendering
{
    public readonly struct PuzzlerFont
    {
        public string Family { get; }
        public float Size { get; }
        public bool Bold { get; }

        public PuzzlerFont(string family, float size, bool bold = false)
        {
            Family = family;
            Size = size;
            Bold = bold;
        }

        public static readonly PuzzlerFont DefaultSerif     = new("Serif", 16, false);
        public static readonly PuzzlerFont DefaultSerifBold = new("Serif", 32, true);
    }
}
