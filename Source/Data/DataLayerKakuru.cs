using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using Data.DataModels;

namespace Data
{
    public class DataLayerKakuru : DataLayerGeneric<PuzzleKakuru>
    {
        public DataLayerKakuru()
        {
            this.PuzzleName = "Kakuru";
        }

        public override PuzzleKakuru? TextToPuzzleObject(string text)
        {
            return ParsePuzzleFromText_1(text);
        }

        public override PuzzleKakuru? WebToPuzzleObject(string url)
        {
            string? text = ReadAndParseKakuruPuzzleFromWebPage(url);
            return text != null ? ParsePuzzleFromText_1(text) : null;
        }

        public static PuzzleKakuru? ParsePuzzleFromText_1(string fileData)
        {
            fileData = fileData.Replace("\r", "");
            List<string> dataLines = new List<string>(fileData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
            if (dataLines.Count == 0) return null;

            List<List<string>> mat = new List<List<string>>();
            foreach (string line in dataLines)
            {
                List<string> cells = new List<string>(line.Split(new char[] { '\t' }, StringSplitOptions.None));
                mat.Add(cells);
            }

            int n = mat.Count;
            int m = mat[0].Count;

            var puzzle = new PuzzleKakuru
            {
                Rows = n,
                Columns = m,
                NumberRange = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            };

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    string cell = mat[i][j];
                    if (cell == "") continue;

                    string[] kakuruLinesValues = cell.Split(new char[] { ',' }, StringSplitOptions.None);
                    string vertical = kakuruLinesValues[0];
                    string horizontal = kakuruLinesValues[1];

                    if (!string.IsNullOrEmpty(vertical))
                    {
                        int index = i + 1;
                        int size = 0;
                        while (index < n && mat[index][j] == "") { index++; size++; }

                        puzzle.SumLines.Add(new DefinedGroupKakuru
                        {
                            RowI = i,
                            ColumnI = j,
                            HorizontalVertical = true,
                            Sum = Convert.ToInt32(vertical),
                            Size = size,
                        });
                    }

                    if (!string.IsNullOrEmpty(horizontal))
                    {
                        int index = j + 1;
                        int size = 0;
                        while (index < m && mat[i][index] == "") { index++; size++; }

                        puzzle.SumLines.Add(new DefinedGroupKakuru
                        {
                            RowI = i,
                            ColumnI = j,
                            HorizontalVertical = false,
                            Sum = Convert.ToInt32(horizontal),
                            Size = size,
                        });
                    }
                }
            }

            return puzzle;
        }

        const string rootBoardNodeID = "CurrentKakuroBoard";
        const string nullNodeName = "cellShaded";
        const string lineNodeName = "cellTotal";
        const string fillNodeName = "cellNumber";
        const string verticalNote = "_v_";
        const string horizontalNote = "_h_";

        public string? ReadAndParseKakuruPuzzleFromWebPage(string url)
        {
            HtmlAgilityPack.HtmlDocument? doc = WebHandler.GetWebPageAsHtmlDocument(url);

            if (doc != null)
            {
                StringBuilder sb = new StringBuilder();

                HtmlNode specificNode = doc.GetElementbyId(rootBoardNodeID);

                HtmlNodeCollection nodesMatchingXPath2 = specificNode.SelectNodes("tr");

                foreach (HtmlNode trNode in nodesMatchingXPath2)
                {
                    HtmlNodeCollection nodesMatchingXPath3 = trNode.SelectNodes("td");

                    foreach (HtmlNode tdNode in nodesMatchingXPath3)
                    {
                        if (tdNode.Attributes["class"].Value == nullNodeName)
                        {
                            sb.Append(",");
                        }
                        else if (tdNode.Attributes["class"].Value == lineNodeName)
                        {
                            HtmlNodeCollection nodesMatchingXPath4 = tdNode.SelectNodes("input");

                            string vertical = "", horizontal = "";
                            foreach (HtmlNode lineNode in nodesMatchingXPath4)
                            {
                                string name = lineNode.Attributes["Name"].Value;

                                if (name.Contains(verticalNote))
                                    vertical = lineNode.Attributes["value"].Value;
                                else if (name.Contains(horizontalNote))
                                    horizontal = lineNode.Attributes["value"].Value;
                            }

                            sb.Append(vertical + "," + horizontal);
                        }
                        else if (tdNode.Attributes["class"].Value == fillNodeName)
                        {
                            sb.Append("");
                        }
                        sb.Append("\t");
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
                if (root.TryGetProperty("Rows", out var rows) && root.TryGetProperty("Columns", out var cols))
                    return $"{rows.GetInt32()}×{cols.GetInt32()}";
                return string.Empty;
            }
            catch { return string.Empty; }
        }
    }
}
