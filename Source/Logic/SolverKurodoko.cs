using System;
using System.Collections.Generic;
using System.Linq;
using Common.Models.Kurodoko;

namespace Logic
{
    public class SolverKurodoko : SolverGeneric<BoardKurodoko>
    {
        // ── validity ──────────────────────────────────────────────────────────

        public override bool IsValid()
        {
            // No two adjacent blacks
            for (int r = 0; r < Board.Rows; r++)
                for (int c = 0; c < Board.Columns; c++)
                {
                    var cell = Board.Cell(r, c);
                    if (cell.Value != true) continue;
                    if (c + 1 < Board.Columns && Board.Cell(r, c + 1).Value == true) return false;
                    if (r + 1 < Board.Rows    && Board.Cell(r + 1, c).Value == true) return false;
                }

            // Clue cells cannot be black
            foreach (var cell in Board.InitialCells)
                if (cell.Value == true) return false;

            // For each clue: min reachable ≤ N ≤ max reachable
            foreach (var cell in Board.InitialCells)
            {
                int n   = cell.ClueNumber!.Value;
                int min = CountMinVisible(cell);
                int max = CountMaxVisible(cell);
                if (min > n || max < n) return false;
            }

            return true;
        }

        public override bool IsSolved()
        {
            // All cells must be decided
            foreach (var cell in Board.ValueCells)
                if (cell.Value == null) return false;

            if (!IsValid()) return false;

            // Every clue sees exactly N whites
            foreach (var cell in Board.InitialCells)
            {
                int n      = cell.ClueNumber!.Value;
                int actual = CountMinVisible(cell); // all decided → min = max = actual
                if (actual != n) return false;
            }

            // All whites form one connected region
            if (!AllWhitesConnected()) return false;

            return true;
        }

        // ── deduction ─────────────────────────────────────────────────────────

        public override bool DoCompleteStep()
        {
            bool progress = false;

            // Rule 1: neighbor of a confirmed black must be white
            progress |= NeighboursOfBlackAreWhite();

            // Rule 2: clue saturation — if max reachable = N, all undecided ray cells must be white
            progress |= ClueSaturationMax();

            // Rule 3: clue saturation — if confirmed whites = N, block the boundaries
            progress |= ClueSaturationMin();

            // Rule 4: blackening a cell would disconnect whites → must be white
            progress |= ConnectivityProtection();

            return progress;
        }

        private bool NeighboursOfBlackAreWhite()
        {
            bool progress = false;
            foreach (var cell in Board.ValueCells)
            {
                if (cell.Value != true) continue;
                foreach (var nb in Board.Neighbors(cell))
                {
                    if (nb.Value == null)
                    {
                        nb.Value = false;
                        progress = true;
                    }
                }
            }
            return progress;
        }

        private bool ClueSaturationMax()
        {
            bool progress = false;
            foreach (var clue in Board.InitialCells)
            {
                int n   = clue.ClueNumber!.Value;
                int max = CountMaxVisible(clue);
                if (max != n) continue;

                // Exactly N whites are reachable optimistically → every undecided cell reachable must be white
                foreach (var (dr, dc) in Directions)
                    foreach (var cell in Board.RayFrom(clue, dr, dc))
                    {
                        if (cell.Value == true) break;
                        if (cell.Value == null)
                        {
                            cell.Value = false;
                            progress   = true;
                        }
                    }
            }
            return progress;
        }

        private bool ClueSaturationMin()
        {
            bool progress = false;
            foreach (var clue in Board.InitialCells)
            {
                int n   = clue.ClueNumber!.Value;
                int min = CountMinVisible(clue);
                if (min < n) continue;

                // min == n: confirmed whites fill the budget; first undecided in each ray must be black
                foreach (var (dr, dc) in Directions)
                    foreach (var cell in Board.RayFrom(clue, dr, dc))
                    {
                        if (cell.Value == true)  break;         // existing black → stop
                        if (cell.Value == null)                 // first undecided → must be black
                        {
                            cell.Value = true;
                            progress   = true;
                            break;
                        }
                        // confirmed white → skip, keep walking
                    }
            }
            return progress;
        }

        private bool ConnectivityProtection()
        {
            bool progress = false;
            foreach (var cell in Board.ValueCells)
            {
                if (cell.Value != null) continue;

                // Temporarily blacken and test connectivity
                cell.Value = true;
                bool disconnects = !AllWhitesConnected();
                cell.Value = null;

                if (disconnects)
                {
                    cell.Value = false;
                    progress   = true;
                }
            }
            return progress;
        }

        // Fast 3-rule sweep used by the generator (skips ConnectivityProtection)
        public bool LightweightDeduce()
            => NeighboursOfBlackAreWhite() | ClueSaturationMax() | ClueSaturationMin();

        // ── backtracking ──────────────────────────────────────────────────────

        private sealed record KurodokoSnapshot(Dictionary<CellValueKurodoko, bool?> CellValues);

        protected override object TakeSnapshot()
        {
            var values = new Dictionary<CellValueKurodoko, bool?>();
            foreach (var cell in Board.ValueCells)
                values[cell] = cell.Value;
            return new KurodokoSnapshot(values);
        }

        protected override void RestoreSnapshot(object snapshot)
        {
            var s = (KurodokoSnapshot)snapshot;
            foreach (var kvp in s.CellValues)
                kvp.Key.Value = kvp.Value;
        }

        protected override IEnumerable<Action> GetBranches()
        {
            // Pick the undecided cell with the most decided neighbours (most constrained)
            CellValueKurodoko? best = null;
            int bestScore = -1;
            foreach (var cell in Board.ValueCells)
            {
                if (cell.Value != null) continue;
                int score = 0;
                foreach (var nb in Board.Neighbors(cell))
                    if (nb.Value != null) score++;
                if (score > bestScore)
                {
                    bestScore = score;
                    best      = cell;
                }
            }

            if (best == null) yield break;

            var target = best;
            yield return () => target.Value = true;
            yield return () => target.Value = false;
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private static readonly (int dr, int dc)[] Directions = { (-1, 0), (1, 0), (0, -1), (0, 1) };

        private int CountMinVisible(CellValueKurodoko clue)
        {
            int count = 1;
            foreach (var (dr, dc) in Directions)
                foreach (var cell in Board.RayFrom(clue, dr, dc))
                {
                    if (cell.Value == true || cell.Value == null) break; // stop at black or undecided
                    count++;
                }
            return count;
        }

        private int CountMaxVisible(CellValueKurodoko clue)
        {
            int count = 1;
            foreach (var (dr, dc) in Directions)
                foreach (var cell in Board.RayFrom(clue, dr, dc))
                {
                    if (cell.Value == true) break; // stop at confirmed black
                    count++; // white or undecided both count optimistically
                }
            return count;
        }

        private bool AllWhitesConnected()
        {
            CellValueKurodoko? start = null;
            int whiteCount = 0;
            foreach (var cell in Board.ValueCells)
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
                foreach (var nb in Board.Neighbors(cur))
                    if (nb.Value != true && visited.Add(nb))
                        queue.Enqueue(nb);
            }
            return visited.Count == whiteCount;
        }
    }
}
