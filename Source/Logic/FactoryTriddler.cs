using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Data.DataModels;
using Logic.Griddler;
using Common.Models.Base;
using Common.Models.Triddler;

namespace Logic.Griddler
{
    public class FactoryTriddler : FactoryGeneric<PuzzleTriddler, BoardTriddler>
    {
        public override BoardTriddler GenerateRandom()
        {
            return GenerateRandom(9, 9);
        }


        protected override BoardTriddler CreateBoardFromPuzzleObject(PuzzleTriddler puzzle)
        {
            BoardTriddler board = new BoardTriddler();

            List<GroupTriddler> HorizontalLines = new List<GroupTriddler>();
            List<GroupTriddler> VerticalLines = new List<GroupTriddler>();
            List<GroupTriddler> DiagonalLines = new List<GroupTriddler>();

            foreach (List<int> row in puzzle.Horizontals)
            {
                GroupTriddlerHorizontal groupRow = new GroupTriddlerHorizontal();
                groupRow.Numbers = row;
                HorizontalLines.Add(groupRow);
            }
            foreach (List<int> column in puzzle.Verticals)
            {
                GroupTriddlerVerical groupColumn = new GroupTriddlerVerical();
                groupColumn.Numbers = column;
                VerticalLines.Add(groupColumn);
            }
            foreach (List<int> diagonal in puzzle.Diagonals)
            {
                GroupTriddlerDiagonal groupDiagonal = new GroupTriddlerDiagonal();
                groupDiagonal.Numbers = diagonal;
                DiagonalLines.Add(groupDiagonal);
            }

            CellValueTriddler[,] CellsMatrixLeft = new CellValueTriddler[puzzle.BaseRowsCount, puzzle.BaseColumnCount];
            for (int i = 0; i < puzzle.BaseRowsCount; i++)
                for (int j = 0; j < puzzle.BaseColumnCount; j++)
                    CellsMatrixLeft[i, j] = new CellValueTriddler(i, j, false);

            CellValueTriddler[,] CellsMatrixRight = new CellValueTriddler[puzzle.BaseRowsCount, puzzle.BaseColumnCount];
            for (int i = 0; i < puzzle.BaseRowsCount; i++)
                for (int j = 0; j < puzzle.BaseColumnCount; j++)
                    CellsMatrixRight[i, j] = new CellValueTriddler(i, j, true);


            //Horizontal
            for (int i = 0; i < puzzle.BaseRowsCount; i++)
            {
                for (int j = 0; j < puzzle.BaseColumnCount; j++)
                {
                    HorizontalLines[i].Cells.Add(CellsMatrixLeft[i, j]); CellsMatrixLeft[i, j].Groups.Add(HorizontalLines[i]);
                    HorizontalLines[i].Cells.Add(CellsMatrixRight[i, j]); CellsMatrixRight[i, j].Groups.Add(HorizontalLines[i]);
                }
            }
            //Vertical
            for (int j = 0; j < puzzle.BaseColumnCount; j++)
            {
                for (int i = 0; i < puzzle.BaseRowsCount; i++)
                {
                    VerticalLines[j].Cells.Add(CellsMatrixRight[i, j]); CellsMatrixRight[i, j].Groups.Add(VerticalLines[j]);
                    VerticalLines[j].Cells.Add(CellsMatrixLeft[i, j]); CellsMatrixLeft[i, j].Groups.Add(VerticalLines[j]);
                }
            }
            //Diagonal
            for (int i = 0; i < puzzle.BaseRowsCount; i++)
            {
                int j = puzzle.BaseColumnCount - 1;
                int ii = i;
                int jj = j;
                bool IsRight = true;
                GroupTriddler group = DiagonalLines[i];
                while (ii >= 0 && jj >= 0)
                {
                    if (IsRight)
                    { group.Cells.Add(CellsMatrixRight[ii, jj]); CellsMatrixRight[ii, jj].Groups.Add(group); ii--; }
                    else
                    { group.Cells.Add(CellsMatrixLeft[ii, jj]); CellsMatrixLeft[ii, jj].Groups.Add(group); jj--; }
                    IsRight = !IsRight;
                }
            }
            for (int j = puzzle.BaseColumnCount - 1; j >= 0; j--)
            {
                int i = puzzle.BaseRowsCount - 1;
                int ii = i;
                int jj = j;
                bool IsRight = false;
                GroupTriddler group = DiagonalLines[i + puzzle.BaseColumnCount - j];
                while (ii >= 0 && jj >= 0)
                {
                    if (IsRight)
                    { group.Cells.Add(CellsMatrixRight[ii, jj]); CellsMatrixRight[ii, jj].Groups.Add(group); ii--; }
                    else
                    { group.Cells.Add(CellsMatrixLeft[ii, jj]); CellsMatrixRight[ii, jj].Groups.Add(group); jj--; }
                    IsRight = !IsRight;
                }
            }


            board.CellsMatrixLeft = CellsMatrixLeft;
            board.CellsMatrixRight = CellsMatrixRight;
            board.Groups.AddRange(HorizontalLines);
            board.Groups.AddRange(VerticalLines);
            board.Groups.AddRange(DiagonalLines);

            //TODO: apply Trimming of diagonal groups ("denting") by (n2,m2), to the board.
            //for N2 remove all cells in Diagonal last group in list
            //for M2 remove all cells in Diagonal first group in list
            List<GroupTriddler> removed = DiagonalLines.GetRange(0, puzzle.N2);
            removed.AddRange(DiagonalLines.GetRange(DiagonalLines.Count - puzzle.M2, puzzle.M2));

            List<CellValueTriddler> removedCells = new List<CellValueTriddler>();
            foreach (var group in removed)
                foreach (var cell in group.Cells)
                {
                    for (int i = 0; i < CellsMatrixLeft.GetLength(0); i++)
                        for (int j = 0; j < CellsMatrixLeft.GetLength(1); j++)
                            if (object.ReferenceEquals(CellsMatrixLeft[i, j], cell))
                            {
                                removedCells.Add(CellsMatrixLeft[i, j]);
                                CellsMatrixLeft[i, j] = null;

                            }
                    for (int i = 0; i < CellsMatrixRight.GetLength(0); i++)
                        for (int j = 0; j < CellsMatrixRight.GetLength(1); j++)
                            if (object.ReferenceEquals(CellsMatrixRight[i, j], cell))
                            {
                                removedCells.Add(CellsMatrixRight[i, j]);
                                CellsMatrixRight[i, j] = null;
                            }
                }

            foreach (var cell in removedCells)
                foreach (var group in board.Groups)
                    if (group.Cells.Contains(cell))
                        group.Cells.Remove(cell);

            foreach (var group in removed)
                board.Groups.Remove(group);

            return board;

        }

        protected override PuzzleTriddler CreatePuzzleObjectFromBoard(BoardTriddler board)
        {
            PuzzleTriddler puzzle = new PuzzleTriddler();

            puzzle.BaseColumnCount = board.Columns;
            puzzle.BaseRowsCount = board.Rows;
            //puzzle.N = board.N ;
            //puzzle.N2 = board.N2;
            //puzzle.M = board.M ;
            //puzzle.M2 = board.M2;
            puzzle.Horizontals = new List<List<int>>();
            puzzle.Verticals = new List<List<int>>();
            puzzle.Diagonals = new List<List<int>>();

            foreach (GroupTriddler group in board.Groups)
            {
                if (group is GroupTriddlerHorizontal)
                {
                    puzzle.Horizontals.Add(group.Numbers);
                }
                else if (group is GroupTriddlerVerical)
                {
                    puzzle.Verticals.Add(group.Numbers);
                }
                else if (group is GroupTriddlerDiagonal)
                {
                    puzzle.Diagonals.Add(group.Numbers);
                }
                else { }
            }
            //board.

            throw new Exception("The method's Implementation is not complete yet.");
        }



        private BoardTriddler GenerateRandom(int rows, int columns)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        
    }
}
