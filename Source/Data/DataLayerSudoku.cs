using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using Data.DataModels;

namespace Data
{
    public class DataLayerSudoku : DataLayerGeneric<SudokuPuzzle>
    {
        public DataLayerSudoku()
        {
            this.PuzzleName = "Sudoku";
        }

        public override SudokuPuzzle? TextToPuzzleObject(string text)
        {
            return ParsePuzzleFromText_2(text);
        }

        public override SudokuPuzzle? WebToPuzzleObject(string url)
        {
            string? text = ReadAndParseSudokuPuzzleFromWebPage(url);
            return text != null ? ParsePuzzleFromText_2(text) : null;
        }

        public static SudokuPuzzle? ParsePuzzleFromText_2(string text2)
        {
            string[] lines = text2.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int size = lines.Length;
            if (size == 0) return null;

            var puzzle = new SudokuPuzzle
            {
                N = (int)Math.Floor(Math.Sqrt(size)),
                M = (int)Math.Ceiling(Math.Sqrt(size)),
            };

            for (int i = 0; i < size; i++)
            {
                string[] cells = lines[i].Split(new char[] { ' ', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < cells.Length; j++)
                {
                    if (cells[j] != ".")
                        puzzle.FixedNumbers.Add(new FixedCellSudoku(i, j, Convert.ToInt32(cells[j]) - 1));
                }
            }

            return puzzle;
        }

        public string? ReadAndParseSudokuPuzzleFromWebPage(string url)
        {
            HtmlAgilityPack.HtmlDocument? doc = WebHandler.GetWebPageAsHtmlDocument(url);

            if (doc != null)
            {
                StringBuilder sb = new StringBuilder();

                HtmlNode specificNode = doc.GetElementbyId("CurrentSudokuBoard");

                HtmlNodeCollection nodesMatchingXPath2 = specificNode.SelectNodes("tr");

                foreach (HtmlNode trNode in nodesMatchingXPath2)
                {
                    HtmlNodeCollection nodesMatchingXPath3 = trNode.SelectNodes("td");

                    foreach (HtmlNode tdNode in nodesMatchingXPath3)
                    {
                        if (string.IsNullOrEmpty(tdNode.InnerText))
                            sb.Append(".");
                        else
                            sb.Append(tdNode.InnerText);

                        sb.Append(" ");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("\n");
                }

                return sb.ToString();
            }
            else
                return null;
        }

        public override string GetPuzzleSizeLabel(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                using var doc = JsonDocument.Parse(stream);
                var root = doc.RootElement;
                if (root.TryGetProperty("N", out var n) && root.TryGetProperty("M", out var m))
                    return $"{n.GetInt32()}×{m.GetInt32()}";
                return string.Empty;
            }
            catch { return string.Empty; }
        }
    }
}
