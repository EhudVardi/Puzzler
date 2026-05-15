using System.Collections.Generic;

namespace Data.DataModels
{
    public class KurodokoPuzzle : PuzzleBase
    {
        public int Rows    { get; set; }
        public int Columns { get; set; }
        public List<KurodokoClue> Clues { get; set; } = new();
    }

    public class KurodokoClue
    {
        public int Row    { get; set; }
        public int Column { get; set; }
        public int Number { get; set; }
    }
}
