namespace Common.Models.Yakugo
{
    public enum YGDirection { Right, Down, Left, Up }

    public static class YGDirectionExtensions
    {
        public static (int dRow, int dCol) Delta(this YGDirection d) => d switch
        {
            YGDirection.Right => (0,  1),
            YGDirection.Down  => (1,  0),
            YGDirection.Left  => (0, -1),
            YGDirection.Up    => (-1, 0),
            _ => (0, 0),
        };

        public static string Glyph(this YGDirection d) => d switch
        {
            YGDirection.Right => "→",
            YGDirection.Down  => "↓",
            YGDirection.Left  => "←",
            YGDirection.Up    => "↑",
            _ => "?",
        };
    }
}
