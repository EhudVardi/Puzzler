using System;
using System.Collections.Generic;
using Common.Models.Kurodoko;
using Data.DataModels;

namespace Logic.Kurodoko
{
    public class FactoryKurodoko : FactoryGeneric<KurodokoPuzzle, BoardKurodoko>
    {
        protected override BoardKurodoko CreateBoardFromPuzzleObject(KurodokoPuzzle puzzle)
        {
            var board = new BoardKurodoko();
            board.SetDimensions(puzzle.Rows, puzzle.Columns);

            var matrix = new CellValueKurodoko[puzzle.Rows, puzzle.Columns];
            for (int r = 0; r < puzzle.Rows; r++)
                for (int c = 0; c < puzzle.Columns; c++)
                    matrix[r, c] = new CellValueKurodoko(r, c);
            board.CellsMatrix = matrix;

            var groups = new List<GroupKurodoko>();

            for (int r = 0; r < puzzle.Rows; r++)
            {
                var cells = new List<CellValueKurodoko>();
                for (int c = 0; c < puzzle.Columns; c++)
                    cells.Add(board.Cell(r, c));
                var group = new GroupKurodoko(cells);
                groups.Add(group);
                foreach (var cell in cells)
                    cell.Groups.Add(group);
            }

            for (int c = 0; c < puzzle.Columns; c++)
            {
                var cells = new List<CellValueKurodoko>();
                for (int r = 0; r < puzzle.Rows; r++)
                    cells.Add(board.Cell(r, c));
                var group = new GroupKurodoko(cells);
                groups.Add(group);
                foreach (var cell in cells)
                    cell.Groups.Add(group);
            }

            board.Groups = groups;

            foreach (var clue in puzzle.Clues)
            {
                var cell = board.Cell(clue.Row, clue.Column);
                cell.ClueNumber = clue.Number;
                cell.Value = false;
                cell.SetFixed(true);
                board.InitialCells.Add(cell);
            }

            return board;
        }

        protected override KurodokoPuzzle CreatePuzzleObjectFromBoard(BoardKurodoko board)
        {
            var puzzle = new KurodokoPuzzle { Rows = board.Rows, Columns = board.Columns };
            foreach (var cell in board.InitialCells)
                puzzle.Clues.Add(new KurodokoClue { Row = cell.Row, Column = cell.Column, Number = cell.ClueNumber!.Value });
            return puzzle;
        }

        public override BoardKurodoko GenerateRandom() => GenerateRandom(20, 20);

        private BoardKurodoko GenerateRandom(int rows, int cols)
        {
            var rand = new Random();

            while (true)
            {
                var board = CreateBoardFromPuzzleObject(new KurodokoPuzzle { Rows = rows, Columns = cols });

                // Randomly blacken cells while keeping adjacency + connectivity valid
                var candidates = new List<CellValueKurodoko>(board.ValueCells);
                Shuffle(candidates, rand);

                foreach (var cell in candidates)
                {
                    if (rand.NextDouble() < 0.25)
                    {
                        cell.Value = true; // try black
                        if (!NoAdjacentBlacks(board) || !AllWhitesConnected(board))
                            cell.Value = null; // revert
                    }
                    else
                    {
                        cell.Value = null;
                    }
                }

                // Fill remaining undecided as white
                foreach (var cell in board.ValueCells)
                    if (cell.Value == null)
                        cell.Value = false;

                if (!AllWhitesConnected(board)) continue;

                // Assign clue numbers to white cells
                var whiteCells = new List<CellValueKurodoko>();
                foreach (var cell in board.ValueCells)
                    if (cell.Value == false)
                        whiteCells.Add(cell);

                Shuffle(whiteCells, rand);
                int clueCount = Math.Max(rows + cols, whiteCells.Count / 4);

                var puzzle = new KurodokoPuzzle { Rows = rows, Columns = cols };
                for (int i = 0; i < Math.Min(clueCount, whiteCells.Count); i++)
                {
                    var cell = whiteCells[i];
                    int vis = CountVisibleWhites(board, cell);
                    puzzle.Clues.Add(new KurodokoClue { Row = cell.Row, Column = cell.Column, Number = vis });
                }

                // Reset board and rebuild with clues only, then verify solver finds unique solution
                var finalBoard = CreateBoardFromPuzzleObject(puzzle);
                var solver = new SolverKurodoko { Board = finalBoard };
                solver.SolveBoard();
                if (solver.IsSolved())
                    return finalBoard;
            }
        }

        private static int CountVisibleWhites(BoardKurodoko board, CellValueKurodoko cell)
        {
            int count = 1; // self
            foreach (var (dr, dc) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                foreach (var neighbor in board.RayFrom(cell, dr, dc))
                {
                    if (neighbor.Value == true) break; // black blocks
                    count++;
                }
            return count;
        }

        private static bool NoAdjacentBlacks(BoardKurodoko board)
        {
            for (int r = 0; r < board.Rows; r++)
                for (int c = 0; c < board.Columns; c++)
                {
                    var cell = board.Cell(r, c);
                    if (cell.Value != true) continue;
                    if (c + 1 < board.Columns && board.Cell(r, c + 1).Value == true) return false;
                    if (r + 1 < board.Rows    && board.Cell(r + 1, c).Value == true) return false;
                }
            return true;
        }

        private static bool AllWhitesConnected(BoardKurodoko board)
        {
            CellValueKurodoko? start = null;
            int whiteCount = 0;
            foreach (var cell in board.ValueCells)
            {
                if (cell.Value != true)
                {
                    whiteCount++;
                    start ??= cell;
                }
            }
            if (start == null) return true;

            var visited = new HashSet<CellValueKurodoko>();
            var queue   = new Queue<CellValueKurodoko>();
            queue.Enqueue(start);
            visited.Add(start);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var nb in board.Neighbors(cur))
                    if (nb.Value != true && visited.Add(nb))
                        queue.Enqueue(nb);
            }
            return visited.Count == whiteCount;
        }

        private static void Shuffle<T>(List<T> list, Random rand)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
