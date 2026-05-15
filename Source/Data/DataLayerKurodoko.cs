using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Data.DataModels;

namespace Data
{
    public class DataLayerKurodoko : DataLayerGeneric<KurodokoPuzzle>
    {
        public DataLayerKurodoko()
        {
            this.PuzzleName = "Kurodoko";
        }

        public override KurodokoPuzzle? TextToPuzzleObject(string text)
        {
            var lines = new List<string[]>();
            foreach (var line in text.Split('\n'))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                lines.Add(trimmed.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
            }

            if (lines.Count == 0) return null;

            int rows    = lines.Count;
            int columns = lines[0].Length;

            var puzzle = new KurodokoPuzzle { Rows = rows, Columns = columns };

            for (int r = 0; r < rows; r++)
            {
                string[] tokens = lines[r];
                for (int c = 0; c < Math.Min(tokens.Length, columns); c++)
                {
                    if (tokens[c] == ".") continue;
                    if (int.TryParse(tokens[c], out int num))
                        puzzle.Clues.Add(new KurodokoClue { Row = r, Column = c, Number = num });
                }
            }

            return puzzle;
        }

        public override string GetPuzzleSizeLabel(string filePath)
        {
            try
            {
                var puzzle = LoadPuzzle(filePath);
                return $"{puzzle.Rows}×{puzzle.Columns}";
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
