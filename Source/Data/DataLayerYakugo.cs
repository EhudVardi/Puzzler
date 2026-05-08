using System.IO;
using System.Text.Json;
using Data.DataModels;

namespace Data
{
    public class DataLayerYakugo : DataLayerGeneric<PuzzleYakugo>
    {
        public DataLayerYakugo()
        {
            this.PuzzleName = "Yakugo";
        }

        public override PuzzleYakugo? TextToPuzzleObject(string text) => null;
        public override PuzzleYakugo? WebToPuzzleObject(string url)   => null;

        public override string GetPuzzleSizeLabel(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                using var doc = JsonDocument.Parse(stream);
                var root = doc.RootElement;
                if (root.TryGetProperty("Rows", out var rows) &&
                    root.TryGetProperty("Cols", out var cols))
                    return $"{rows.GetInt32()}×{cols.GetInt32()}";
                return string.Empty;
            }
            catch { return string.Empty; }
        }
    }
}
