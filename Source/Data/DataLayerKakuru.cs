using System;
using System.Collections.Generic;
using System.Text;
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


        public override PuzzleKakuru TextToPuzzleObject(string text)
        {
            string tempFileName = Guid.NewGuid().ToString();
            string puzzleXmlData = ParsePuzzleStringFromText_1(text);
            if (puzzleXmlData != null)
            {
                System.IO.File.WriteAllText(tempFileName, puzzleXmlData);
                return base.XMLToPuzzle(tempFileName);
            }
            else
                return null;
        }

        public override PuzzleKakuru WebToPuzzleObject(string url)
        {
            string tempFileName = Guid.NewGuid().ToString();
            System.IO.File.WriteAllText(tempFileName, ParsePuzzleStringFromText_1(ReadAndParseKakuruPuzzleFromWebPage(url)));

            return XMLToPuzzle(tempFileName);
        }





        //TODO: convert createFormat# functions to create a puzzle object directly from a given text in a given format


        static string kakuruMainHeader = "<?xml version=\"1.0\"?>\n" +
                        "<KakuruPuzzle xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";

        static string RowsHeader = "<Rows>";
        static string RowsFooter = "</Rows>";

        static string ColumnsHeader = "<Columns>";
        static string ColumnsFooter = "</Columns>";

        static string NumberRangeHeader = "<NumberRange>";
        static string NumberHeader = "<int>";
        static string NumberFooter = "</int>";
        static string NumberRangeFooter = "</NumberRange>";

        static string SumLinesHeader = "<SumLines>";

        static string KakuruSumLineHeader = "<KakuruSumLine>";

        static string RowIHeader = "<RowI>";
        static string RowIFooter = "</RowI>";

        static string ColumnIHeader = "<ColumnI>";
        static string ColumnIFooter = "</ColumnI>";

        static string HorizontalVerticalHeader = "<HorizontalVertical>";
        static string HorizontalVerticalFooter = "</HorizontalVertical>";

        static string SumHeader = "<Sum>";
        static string SumFooter = "</Sum>";

        static string SizeHeader = "<Size>";
        static string SizeFooter = "</Size>";

        static string KakuruSumLineFooter = "</KakuruSumLine>";

        static string SumLinesFooter = "</SumLines>";

        static string kakuruMainFooter = "</KakuruPuzzle>";
        //




        public static string ParsePuzzleStringFromText_1(string fileData)
        {
            try
            {

                string xmlDocument = "";

                xmlDocument += kakuruMainHeader + "\n";

                fileData = fileData.Replace("\r", "");
                List<string> dataLines = new List<string>(fileData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

                List<List<string>> mat = new List<List<string>>();
                foreach (string line in dataLines)
                {
                    List<string> cells = new List<string>(line.Split(new char[] { '\t' }, StringSplitOptions.None));
                    mat.Add(cells);
                }

                int n = mat.Count;
                int m = mat[0].Count;//all rows should be the same size.

                xmlDocument += RowsHeader + n + RowsFooter + "\n";
                xmlDocument += ColumnsHeader + m + ColumnsFooter + "\n";

                xmlDocument += NumberRangeHeader + "\n";
                for (int i = 1; i < 10; i++)
                {
                    xmlDocument += NumberHeader + i.ToString() + NumberFooter + "\n";
                }
                xmlDocument += NumberRangeFooter + "\n";


                xmlDocument += SumLinesHeader + "\n";

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < m; j++)
                    {
                        string cell = mat[i][j];

                        if (cell == "") { } //player filling cell
                        else //line (right & down are optional, so it can be void) cell
                        {
                            string[] kakuruLinesValues = cell.Split(new char[] { ',' }, StringSplitOptions.None);
                            string vertical = kakuruLinesValues[0];
                            string horizontal = kakuruLinesValues[1];

                            if (!string.IsNullOrEmpty(vertical))
                            {
                                xmlDocument += KakuruSumLineHeader + "\n";

                                xmlDocument += RowIHeader + i.ToString() + RowIFooter + "\n";

                                xmlDocument += ColumnIHeader + j.ToString() + ColumnIFooter + "\n";

                                xmlDocument += HorizontalVerticalHeader + "true" + HorizontalVerticalFooter + "\n";

                                xmlDocument += SumHeader + Convert.ToInt32(vertical) + SumFooter + "\n";

                                int index = i + 1;
                                int size = 0;
                                while (index < n && mat[index][j] == "")
                                {
                                    index++;
                                    size++;
                                }
                                xmlDocument += SizeHeader + size.ToString() + SizeFooter + "\n";

                                xmlDocument += KakuruSumLineFooter + "\n";
                            }
                            if (!string.IsNullOrEmpty(horizontal))
                            {
                                xmlDocument += KakuruSumLineHeader + "\n";

                                xmlDocument += RowIHeader + i.ToString() + RowIFooter + "\n";

                                xmlDocument += ColumnIHeader + j.ToString() + ColumnIFooter + "\n";

                                xmlDocument += HorizontalVerticalHeader + "false" + HorizontalVerticalFooter + "\n";

                                xmlDocument += SumHeader + Convert.ToInt32(horizontal) + SumFooter + "\n";

                                int index = j + 1;
                                int size = 0;
                                while (index < m && mat[i][index] == "")
                                {
                                    index++;
                                    size++;
                                }
                                xmlDocument += SizeHeader + size.ToString() + SizeFooter + "\n";

                                xmlDocument += KakuruSumLineFooter + "\n";

                            }
                        }
                    }
                }

                xmlDocument += SumLinesFooter + "\n";


                xmlDocument += kakuruMainFooter;


                return xmlDocument;


            }
            catch (Exception ex)
            {
                return null;
            }

        }




        const string rootBoardNodeID = "CurrentKakuroBoard";

        const string nullNodeName = "cellShaded";
        const string lineNodeName = "cellTotal";
        const string fillNodeName = "cellNumber";

        const string verticalNote = "_v_";
        const string horizontalNote = "_h_";



        public string ReadAndParseKakuruPuzzleFromWebPage(string url)
        {
            HtmlAgilityPack.HtmlDocument doc = WebHandler.GetWebPageAsHtmlDocument(url);

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

                                if (name.Contains(verticalNote)) //vertical line
                                {
                                    vertical = lineNode.Attributes["value"].Value;
                                }
                                else if (name.Contains(horizontalNote)) //horizontal line
                                {
                                    horizontal = lineNode.Attributes["value"].Value;
                                }
                                else
                                {
                                    //not valid. should be empty
                                }

                            }

                            sb.Append(vertical + "," + horizontal);

                        }
                        else if (tdNode.Attributes["class"].Value == fillNodeName)
                        {
                            sb.Append("");
                        }
                        else
                        {

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


    }
}
