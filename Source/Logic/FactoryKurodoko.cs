using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.Models.Kurodoko;
using Data.DataModels;

namespace Logic.Kurodoko
{
    public enum KurodokoGenMode
    {
        Minimal,        // remove every removable clue (most sparse, slowest)
        Moderate,       // process only the first half of candidates (faster, denser)
        TimeBoundedMin, // Minimal with a wall-clock cap — returns best achieved so far
    }

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

        public override BoardKurodoko GenerateRandom() =>
            GenerateRandom(19, 11, KurodokoGenMode.TimeBoundedMin, timeBudgetMs: 10_000);

        private BoardKurodoko GenerateRandom(int rows, int cols,
            KurodokoGenMode mode = KurodokoGenMode.TimeBoundedMin, int timeBudgetMs = 10_000)
        {
            var rand     = new Random();
            var solution = GenerateSolutionGrid(rows, cols, rand);
            var puzzle   = BuildAllCluesPuzzle(solution, rows, cols);

            var candidates = new List<KurodokoClue>(puzzle.Clues);
            Shuffle(candidates, rand);

            int totalCandidates = candidates.Count;
            int trialsLimit     = mode == KurodokoGenMode.Moderate
                ? totalCandidates / 2
                : totalCandidates;

            var sw = Stopwatch.StartNew();
            int trials = 0;

            foreach (var candidate in candidates)
            {
                if (trials++ >= trialsLimit) break;
                if (mode == KurodokoGenMode.TimeBoundedMin && sw.ElapsedMilliseconds > timeBudgetMs) break;

                puzzle.Clues.Remove(candidate);
                if (!TryDeduceAgainstSolution(puzzle, solution, rows, cols))
                    puzzle.Clues.Add(candidate); // restore — removal broke unique deducibility
            }

            return CreateBoardFromPuzzleObject(puzzle);
        }

        // ── phase-1: generate a valid black/white solution grid ───────────────

        private static bool[,] GenerateSolutionGrid(int rows, int cols, Random rand)
        {
            var solution     = new bool[rows, cols]; // false = white initially
            int targetBlacks = Math.Max(1, (int)(rows * cols * 0.22));
            int placed       = 0;

            var positions = new List<(int r, int c)>(rows * cols);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    positions.Add((r, c));
            Shuffle(positions, rand);

            foreach (var (r, c) in positions)
            {
                if (placed >= targetBlacks) break;
                solution[r, c] = true;
                bool adj = (r > 0        && solution[r - 1, c]) ||
                           (r < rows - 1 && solution[r + 1, c]) ||
                           (c > 0        && solution[r, c - 1]) ||
                           (c < cols - 1 && solution[r, c + 1]);
                if (adj || !AllWhitesConnectedArr(solution, rows, cols))
                    solution[r, c] = false;
                else
                    placed++;
            }
            return solution;
        }

        // ── phase-2 helpers: build full-clue puzzle & test clue removal ───────

        private static KurodokoPuzzle BuildAllCluesPuzzle(bool[,] solution, int rows, int cols)
        {
            var puzzle = new KurodokoPuzzle { Rows = rows, Columns = cols };
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (!solution[r, c])
                        puzzle.Clues.Add(new KurodokoClue
                        {
                            Row    = r,
                            Column = c,
                            Number = CountVisibleArr(solution, rows, cols, r, c),
                        });
            return puzzle;
        }

        private bool TryDeduceAgainstSolution(KurodokoPuzzle puzzle, bool[,] solution, int rows, int cols)
        {
            var board  = CreateBoardFromPuzzleObject(puzzle);
            var solver = new SolverKurodoko { Board = board };

            while (solver.LightweightDeduce()) { }

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    var cell = board.Cell(r, c);
                    if (cell.Value == null) return false;
                    if (cell.Value != solution[r, c]) return false;
                }
            return true;
        }

        // ── array-based helpers (work on bool[,], no BoardKurodoko overhead) ──

        private static bool AllWhitesConnectedArr(bool[,] grid, int rows, int cols)
        {
            int sr = -1, sc = -1, whiteCount = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (!grid[r, c]) { whiteCount++; if (sr < 0) { sr = r; sc = c; } }
            if (sr < 0) return true;

            var visited = new bool[rows, cols];
            var queue   = new Queue<(int, int)>();
            queue.Enqueue((sr, sc));
            visited[sr, sc] = true;
            int count = 1;
            while (queue.Count > 0)
            {
                var (r, c) = queue.Dequeue();
                foreach (var (nr, nc) in new[] { (r - 1, c), (r + 1, c), (r, c - 1), (r, c + 1) })
                {
                    if (nr < 0 || nr >= rows || nc < 0 || nc >= cols) continue;
                    if (!grid[nr, nc] && !visited[nr, nc])
                    {
                        visited[nr, nc] = true;
                        count++;
                        queue.Enqueue((nr, nc));
                    }
                }
            }
            return count == whiteCount;
        }

        private static int CountVisibleArr(bool[,] grid, int rows, int cols, int row, int col)
        {
            int count = 1;
            foreach (var (dr, dc) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
            {
                int r = row + dr, c = col + dc;
                while (r >= 0 && r < rows && c >= 0 && c < cols && !grid[r, c])
                {
                    count++;
                    r += dr;
                    c += dc;
                }
            }
            return count;
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
