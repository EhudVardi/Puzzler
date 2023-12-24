using System;
using System.Collections.Generic;
using System.Text;
using Data.DataModels;

namespace Data
{
    public class DataLayerGriddler : DataLayerGeneric<PuzzleGriddler>
    {
        public DataLayerGriddler()
        {
            this.PuzzleName = "Griddler";
        }



        public override PuzzleGriddler TextToPuzzleObject(string text)
        {
            return StringToGriddlerPuzzle_Format1(text);
        }

        public override PuzzleGriddler WebToPuzzleObject(string url)
        {
            return null;
        }





        private PuzzleGriddler StringToGriddlerPuzzle_Format1(string text)
        {
            PuzzleGriddler puzzle = null;

            if (!string.IsNullOrEmpty(text))
            {
                string[] rowsAndColumns = text.Split(new string[] { "\r\n-\r\n" }, StringSplitOptions.None);
                if (rowsAndColumns.Length == 2)
                {
                    string rowsData = rowsAndColumns[0];
                    string columnsData = rowsAndColumns[1];

                    string[] rows = rowsData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] columns = columnsData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    if (rows.Length > 0 && columns.Length > 0)
                    {
                        puzzle = new PuzzleGriddler();

                        puzzle.RowsLength = columns.Length;
                        puzzle.ColumnLength = rows.Length;

                        puzzle.Rows = new List<List<int>>();
                        foreach (string row in rows)
                        {
                            List<int> rowList = new List<int>();
                            string[] rowValues = row.Split(new char[] { ',' });
                            foreach (string value in rowValues)
                            {
                                int num = Convert.ToInt32(value);
                                rowList.Add(num);
                            }
                            puzzle.Rows.Add(rowList);
                        }
                        puzzle.Columns = new List<List<int>>();
                        foreach (string column in columns)
                        {
                            List<int> columnList = new List<int>();
                            string[] columnNumbers = column.Split(new char[] { ',' });
                            foreach (string number in columnNumbers)
                            {
                                int num = Convert.ToInt32(number);
                                columnList.Add(num);
                            }
                            puzzle.Columns.Add(columnList);
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
