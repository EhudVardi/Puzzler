using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;

namespace Data
{
    public class DataLayerTriddler : DataLayerGeneric<PuzzleTriddler>
    {
        public DataLayerTriddler()
        {
            this.PuzzleName = "Triddler";
        }



        public override PuzzleTriddler TextToPuzzleObject(string text)
        {
            return StringToGriddlerPuzzle_Format1(text);
        }

        public override PuzzleTriddler WebToPuzzleObject(string url)
        {
            return null;
        }





        private PuzzleTriddler StringToGriddlerPuzzle_Format1(string text)
        {
            PuzzleTriddler puzzle = null;

            if (!string.IsNullOrEmpty(text))
            {
                string[] rowsAndColumnsAndDiagonals = text.Split(new string[] { "\r\n-\r\n" }, StringSplitOptions.None);
                if (rowsAndColumnsAndDiagonals.Length == 3)
                {
                    string rowsData = rowsAndColumnsAndDiagonals[0];
                    string columnsData = rowsAndColumnsAndDiagonals[1];
                    string diagonalsData = rowsAndColumnsAndDiagonals[2];

                    string[] rows = rowsData.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    string[] columns = columnsData.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    string[] diagonals = diagonalsData.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    if (rows.Length > 0 && columns.Length > 0 && diagonals.Length > 0)
                    {
                        puzzle = new PuzzleTriddler();

                        puzzle.BaseRowsCount = rows.Length;
                        puzzle.BaseColumnCount = columns.Length;

                        puzzle.Horizontals = new List<List<int>>();
                        foreach (string row in rows)
                        {
                            List<int> rowList = new List<int>();
                            string[] rowValues = row.Split(new char[] { ',' });
                            foreach (string number in rowValues)
                            {
                                int num;
                                if (int.TryParse(number, out num) == true)
                                    rowList.Add(num);
                            }
                            puzzle.Horizontals.Add(rowList);
                        }
                        puzzle.Verticals = new List<List<int>>();
                        foreach (string column in columns)
                        {
                            List<int> columnList = new List<int>();
                            string[] columnNumbers = column.Split(new char[] { ',' });
                            foreach (string number in columnNumbers)
                            {
                                int num;
                                if (int.TryParse(number, out num) == true)
                                    columnList.Add(num);
                            }
                            puzzle.Verticals.Add(columnList);
                        }
                        puzzle.Diagonals = new List<List<int>>();
                        foreach (string diagonal in diagonals)
                        {
                            List<int> diagonalList = new List<int>();
                            string[] diagonalNumbers = diagonal.Split(new char[] { ',' });

                            foreach (string number in diagonalNumbers)
                            {
                                int num;
                                if (int.TryParse(number, out num) == true)
                                    diagonalList.Add(num);
                            }
                            puzzle.Diagonals.Add(diagonalList);
                        }
                    }
                    else
                    {
                        //rows or columns count equals to zero
                    }
                }
                else
                {
                    //data order not match to row list and then column list
                }
            }
            else
            {
                //no data
            }

            return puzzle;
        }

    }
}
