using System;
using System.Collections.Generic;
using System.Text;
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



        public override SudokuPuzzle TextToPuzzleObject(string text)
        {
            string tempFileName = Guid.NewGuid().ToString();
            string puzzleXmlData = ParsePuzzleStringFromText_1(text);
            if (puzzleXmlData != null)
            {
                System.IO.File.WriteAllText(tempFileName, puzzleXmlData);
                return XMLToPuzzle(tempFileName);
            }
            else
                return null;
        }

        public override SudokuPuzzle WebToPuzzleObject(string url)
        {
            string tempFileName = Guid.NewGuid().ToString();
            System.IO.File.WriteAllText(tempFileName, ParsePuzzleStringFromText_2(ReadAndParseSudokuPuzzleFromWebPage(url)));

            return XMLToPuzzle(tempFileName);
        }










        static string SudokuMainHeader = "<?xml version=\"1.0\"?>\n" +
                        "<SudokuPuzzle xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";

        static string sizeHeader = "<Size>";//deprecated
        static string sizeFooter = "</Size>";//deprecated

        static string NHeader = "<N>";
        static string NFooter = "</N>";

        static string MHeader = "<M>";
        static string MFooter = "</M>";

        static string fixedNumbersHeader = "<FixedNumbers>";

        static string sudokuCellFixedNumberHeader = "<SudokuCellFixedNumber>";

        static string rowHeader = "<Row>";
        static string rowFooter = "</Row>";

        static string columnHeader = "<Column>";
        static string columnFooter = "</Column>";

        static string numberHeader = "<Number>";
        static string numberFooter = "</Number>";

        static string sudokuCellFixedNumberFooter = "</SudokuCellFixedNumber>";

        static string fixedNumbersFooter = "</FixedNumbers>";

        static string SudokuMainFooter = "</SudokuPuzzle>";
        //


        public static string ParsePuzzleStringFromText_4(string text2)
        {
            string xmlDocument = "";

            string[] text = text2.Split(new char[] { '\n' });

            xmlDocument += SudokuMainHeader + "\n";

            int size = text.Length;

            xmlDocument += sizeHeader + size.ToString() + sizeFooter + "\n";

            string[] sep2 = text;

            xmlDocument += fixedNumbersHeader + "\n";

            for (int i = 0; i < sep2.Length; i++)
            {
                string[] line = sep2[i].Split(new char[] { '\t' }, StringSplitOptions.None);
                for (int j = 0; j < line.Length; j++)
                {
                    string r = i.ToString(), c = j.ToString(), v = line[j];

                    if (!string.IsNullOrEmpty(v))
                    {
                        xmlDocument += sudokuCellFixedNumberHeader + "\n";

                        xmlDocument += rowHeader + r + rowFooter + "\n";
                        xmlDocument += columnHeader + c + columnFooter + "\n";
                        xmlDocument += numberHeader + Convert.ToString((Convert.ToInt32(v) - 1)) + numberFooter + "\n";

                        xmlDocument += sudokuCellFixedNumberFooter + "\n";
                    }
                }
            }


            xmlDocument += fixedNumbersFooter + "\n";

            xmlDocument += SudokuMainFooter + "\n";


            return xmlDocument;
        }

        public static string ParsePuzzleStringFromText_3(string text2)
        {
            string xmlDocument = "";

            string[] text = text2.Split(new char[] { '\n' });

            xmlDocument += SudokuMainHeader + "\n";

            int size = 9;

            xmlDocument += sizeHeader + size.ToString() + sizeFooter + "\n";

            string[] sep2 = text;

            xmlDocument += fixedNumbersHeader + "\n";

            for (int i = 0; i < sep2.Length; i++)
            {
                string[] line = sep2[i].Split(new char[] { '\t' });
                string r = line[0], c = line[1], v = line[2];
                xmlDocument += sudokuCellFixedNumberHeader + "\n";

                xmlDocument += rowHeader + r.ToString() + rowFooter + "\n";
                xmlDocument += columnHeader + c.ToString() + columnFooter + "\n";
                xmlDocument += numberHeader + Convert.ToString((Convert.ToInt32(v) - 1)) + numberFooter + "\n";

                xmlDocument += sudokuCellFixedNumberFooter + "\n";
            }


            xmlDocument += fixedNumbersFooter + "\n";

            xmlDocument += SudokuMainFooter + "\n";


            return xmlDocument;
        }

        public static string ParsePuzzleStringFromText_2(string text2)
        {
            string xmlDocument = "";

            string[] text = text2.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            xmlDocument += SudokuMainHeader + "\n";

            int size = text.Length;

            xmlDocument += NHeader + Convert.ToInt32(Math.Floor(Math.Sqrt(size))).ToString() + NFooter + "\n";
            xmlDocument += MHeader + Convert.ToInt32(Math.Ceiling(Math.Sqrt(size))).ToString() + MFooter + "\n";

            string[] sep2 = text;

            xmlDocument += fixedNumbersHeader + "\n";

            for (int i = 0; i < size; i++)
            {
                string[] line = sep2[i].Split(new char[] { ' ', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < line.Length; j++)
                {
                    if (line[j] != ".")
                    {
                        xmlDocument += sudokuCellFixedNumberHeader + "\n";

                        xmlDocument += rowHeader + i.ToString() + rowFooter + "\n";
                        xmlDocument += columnHeader + j.ToString() + columnFooter + "\n";
                        xmlDocument += numberHeader + Convert.ToString((Convert.ToInt32(line[j]) - 1)) + numberFooter + "\n";

                        xmlDocument += sudokuCellFixedNumberFooter + "\n";
                    }
                }
            }


            xmlDocument += fixedNumbersFooter + "\n";

            xmlDocument += SudokuMainFooter + "\n";


            return xmlDocument;
        }

        public static string ParsePuzzleStringFromText_1(string text)
        {
            try
            {

                string xmlDocument = "";

                xmlDocument += SudokuMainHeader + "\n";

                string[] sep1 = text.Split(new char[] { '\n' });

                string size = sep1[0];

                xmlDocument += sizeHeader + size.ToString() + sizeFooter + "\n";

                string fixedNumbers = sep1[1];

                string[] sep2 = fixedNumbers.Split(new char[] { '\t' });

                xmlDocument += fixedNumbersHeader + "\n";

                foreach (string fixedNum in sep2)
                {
                    if (!string.IsNullOrEmpty(fixedNum))
                    {
                        xmlDocument += sudokuCellFixedNumberHeader + "\n";

                        string[] sep3 = fixedNum.Split(new char[] { ',' });

                        xmlDocument += rowHeader + sep3[0].ToString() + rowFooter + "\n";
                        xmlDocument += columnHeader + sep3[1].ToString() + columnFooter + "\n";
                        xmlDocument += numberHeader + sep3[2].ToString() + numberFooter + "\n";

                        xmlDocument += sudokuCellFixedNumberFooter + "\n";
                    }
                }

                xmlDocument += fixedNumbersFooter + "\n";

                xmlDocument += SudokuMainFooter + "\n";

                return xmlDocument;
            }
            catch (Exception ex)
            {
                return null;
            }
        }




        public string ReadAndParseSudokuPuzzleFromWebPage(string url)
        {
            HtmlAgilityPack.HtmlDocument doc = WebHandler.GetWebPageAsHtmlDocument(url);

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

    }
}
