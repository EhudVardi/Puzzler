using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Data.DataModels;
using Logic.Griddler;
using Common.Models.Base;
using Common.Models.Griddler;

namespace Logic.Griddler
{
    public class FactoryGriddler : FactoryGeneric<PuzzleGriddler, BoardGriddler>
    {
        public override BoardGriddler GenerateRandom()
        {
            return GenerateRandom(9, 9);
        }


        protected override BoardGriddler CreateBoardFromPuzzleObject(PuzzleGriddler puzzle)
        {
            BoardGriddler board = new BoardGriddler();

            List<GroupGriddler> RowLines = new List<GroupGriddler>();
            List<GroupGriddler> ColumnLines = new List<GroupGriddler>();

            foreach (List<int> row in puzzle.Rows)
            {
                GroupGriddlerRow groupRow = new GroupGriddlerRow();
                groupRow.Numbers = row;
                RowLines.Add(groupRow);
            }
            foreach (List<int> column in puzzle.Columns)
            {
                GroupGriddlerColumn groupColumn = new GroupGriddlerColumn();
                groupColumn.Numbers = column;
                ColumnLines.Add(groupColumn);
            }

            CellValueGriddler[,] CellsMatrix = new CellValueGriddler[puzzle.Rows.Count, puzzle.Columns.Count];

            for (int i = 0; i < puzzle.Rows.Count; i++)
                for (int j = 0; j < puzzle.Columns.Count; j++)
                    CellsMatrix[i, j] = new CellValueGriddler(i, j);


            for (int i = 0; i < puzzle.Rows.Count; i++)
            {
                for (int j = 0; j <  puzzle.Columns.Count; j++)
                {
                    CellsMatrix[i, j].Groups.Add(RowLines[i]);
                    CellsMatrix[i, j].Groups.Add(ColumnLines[j]);

                    RowLines[i].Cells.Add(CellsMatrix[i, j]);
                    ColumnLines[j].Cells.Add(CellsMatrix[i, j]);
                }
            }

            board.CellsMatrix = CellsMatrix;
            board.Groups.AddRange(RowLines);
            board.Groups.AddRange(ColumnLines);


            return board;

        }

        protected override PuzzleGriddler CreatePuzzleObjectFromBoard(BoardGriddler board)
        {
            PuzzleGriddler puzzle = new PuzzleGriddler();

            puzzle.RowsLength = board.Columns;
            puzzle.ColumnLength = board.Rows;

            puzzle.Rows = new List<List<int>>();
            puzzle.Columns = new List<List<int>>();

            //board.

            throw new Exception("The method's Implementation is not complete yet.");
        }



        private BoardGriddler GenerateRandom(int rows, int columns)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        
    }
}
