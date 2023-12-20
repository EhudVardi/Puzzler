using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Data.DataModels;
using Common.Models.Base;
using Common.Models.Sudoku;

namespace Logic.Sudoku
{
    public class FactorySudoku : FactoryGeneric<SudokuPuzzle, BoardSudoku>
    {

        public override BoardSudoku GenerateRandom()
        {
            return GenerateRandom(3, 3);
        }


        protected override BoardSudoku CreateBoardFromPuzzleObject(SudokuPuzzle puzzle)
        {
            BoardSudoku board = new BoardSudoku();

            SetStructureClassic(board, puzzle.N, puzzle.M);

            foreach (FixedCellSudoku fixedNum in puzzle.FixedNumbers)
            {
                board.SetCell(fixedNum.Row, fixedNum.Column, fixedNum.Number);
                board.InitialCells.Add(board.CellsMatrix[fixedNum.Row, fixedNum.Column] as CellValueSudoku);
            }

            return board;
        }

        protected override SudokuPuzzle CreatePuzzleObjectFromBoard(BoardSudoku board)
        {
            SudokuPuzzle puzzle = new SudokuPuzzle();

            puzzle.N = board.N;
            puzzle.M = board.M;

            puzzle.FixedNumbers = new List<FixedCellSudoku>();


            foreach (CellValueSudoku valueCell in board.ValueCells)
            {
                if (valueCell.IsFixed)
                    puzzle.FixedNumbers.Add(new FixedCellSudoku(valueCell.Row, valueCell.Column, (int)valueCell.Value));
            }        

            return puzzle;
        }




        private static void SetStructureClassic(BoardSudoku board, int n, int m)
        {
            board.N = n;
            board.M = m;

            board.CellsMatrix = new CellValueSudoku[n * m, n * m];

            for (int i = 0; i < board.CellsMatrix.GetLength(0); i++)
                for (int j = 0; j < board.CellsMatrix.GetLength(1); j++)
                    board.CellsMatrix[i, j] = new CellValueSudoku(i, j, n * m);

            List<GroupSudoku> rows = new List<GroupSudoku>();
            List<GroupSudoku> columns = new List<GroupSudoku>();
            List<GroupSudoku> boxes = new List<GroupSudoku>();

            for (int i = 0; i < board.CellsMatrix.GetLength(0); i++)
            {
                List<CellValueSudoku> cells = new List<CellValueSudoku>();
                for (int j = 0; j < board.CellsMatrix.GetLength(1); j++)
                    cells.Add(board.CellsMatrix[i, j] as CellValueSudoku);
                
                GroupSudoku sg = new GroupSudoku(cells);
                rows.Add(sg);
            }

            for (int i = 0; i < board.CellsMatrix.GetLength(0); i++)
            {
                List<CellValueSudoku> cells = new List<CellValueSudoku>();
                for (int j = 0; j < board.CellsMatrix.GetLength(1); j++)
                    cells.Add(board.CellsMatrix[j, i] as CellValueSudoku);

                GroupSudoku sg = new GroupSudoku(cells);
                columns.Add(sg);
            }

            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                {
                    List<CellValueSudoku> cells = new List<CellValueSudoku>();
                    for (int x = 0; x < n; x++)
                        for (int y = 0; y < m; y++)
                            cells.Add(board.CellsMatrix[i * n + x, j * m + y] as CellValueSudoku);

                    GroupSudoku sg = new GroupSudoku(cells);
                    boxes.Add(sg);
                }

            board.Groups = new List<GroupSudoku>();
            board.Groups.AddRange(rows);
            board.Groups.AddRange(columns);
            board.Groups.AddRange(boxes);
        }



        private BoardSudoku GenerateRandom(int n, int m)
        {
            BoardSudoku board = new BoardSudoku();

            SetStructureClassic(board, n, m);

            Random rand = new Random();

            int rows = board.CellsMatrix.GetLength(0);
            int columns = board.CellsMatrix.GetLength(1);

            List<KeyValuePair<CellValueSudoku, int>> randomFixedCells = new List<KeyValuePair<CellValueSudoku, int>>();
            List<CellValueSudoku> notSetCells = new List<CellValueSudoku>();
            foreach (CellValueSudoku cell in board.CellsMatrix)
                notSetCells.Add(cell);

            SolverSudoku solver = new SolverSudoku();
            solver.Board = board;

            solver.SolveInitiation();

            while (!solver.IsSolved())
            {

                while (solver.DoCompleteStep()) ;

                notSetCells.Clear();
                foreach (CellValueSudoku cell in board.CellsMatrix)
                    if (!cell.IsFixed)
                        notSetCells.Add(cell);

                if (solver.IsSolved())
                    break;
                else
                {
                    if (!solver.IsValid())
                    {
                        randomFixedCells.RemoveAt(randomFixedCells.Count - 1);

                        solver.Reset();

                        foreach (KeyValuePair<CellValueSudoku, int> fixedCell in randomFixedCells)
                        {
                            solver.Board.SetCell(fixedCell.Key.Row, fixedCell.Key.Column, fixedCell.Value);
                        }
                    }
                    else
                    {
                        CellValueSudoku randomCell = notSetCells[rand.Next(0, notSetCells.Count - 1)];

                        //throw new NotImplementedException();
                        
                        int randomNumberIndex = rand.Next(0, solver.GetChoiceMapForCell(randomCell).Count - 1);
                        while (solver.GetChoiceMapForCell(randomCell).GetSingleBit(randomNumberIndex) == false)
                            randomNumberIndex = rand.Next(0, solver.GetChoiceMapForCell(randomCell).Count - 1);

                        solver.SetCell(randomCell.Row, randomCell.Column, randomNumberIndex);

                        randomFixedCells.Add(new KeyValuePair<CellValueSudoku, int>(randomCell, (int)randomCell.Value));

                        FireStepGenerated(this, EventArgs.Empty);
                    }
                }
            }


            solver.Reset();

            foreach (KeyValuePair<CellValueSudoku, int> fixedCell in randomFixedCells)
                (board.CellsMatrix[fixedCell.Key.Row, fixedCell.Key.Column] as CellValueSudoku).Value = fixedCell.Value;


            return board;
        }



    }
}
