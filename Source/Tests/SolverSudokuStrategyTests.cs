using System.Collections.Generic;
using Common.Models.Sudoku;
using Data.DataModels;
using Logic;
using Logic.Sudoku;
using Xunit;

namespace Tests
{
    // Isolated unit tests for SolverSudoku strategies that may never fire during
    // full-puzzle integration runs (the solver reaches "stuck" via Levels 1-3 first).
    // Each test crafts the minimum board state needed to trigger exactly the target strategy.
    public class SolverSudokuStrategyTests
    {
        // Creates a 9x9 board with no fixed cells and calls SolveInitiation so all
        // internal structures (choice maps, subgroups, row/column indexes) are ready.
        // After this, every cell's choice map has all 9 bits set — a clean slate for
        // manually setting up any desired pattern.
        private static (SolverSudoku solver, BoardSudoku board) EmptyBoard()
        {
            var puzzle = new SudokuPuzzle { N = 3, M = 3, FixedNumbers = new List<FixedCellSudoku>() };
            var board = new FactorySudoku().PuzzleToBoard(puzzle)!;
            var solver = new SolverSudoku { Board = board };
            solver.SolveInitiation();
            return (solver, board);
        }

        private static CellValueSudoku Cell(BoardSudoku board, int row, int col)
            => (CellValueSudoku)board.CellsMatrix[row, col];

        // -------------------------------------------------------------------------
        // Hidden Sets (hidden pairs / triples)
        // -------------------------------------------------------------------------

        [Fact]
        public void FindHiddenSets_HiddenPairInRow_RemovesExtraCandidates()
        {
            var (solver, board) = EmptyBoard();

            // Make candidates 0 and 1 appear ONLY in cells [0,7] and [0,8] of row 0.
            // Clear bits 0 and 1 from all other cells in the row.
            for (int col = 0; col <= 6; col++)
            {
                solver.GetChoiceMapForCell(Cell(board, 0, col)).SetSingleBit(0, false);
                solver.GetChoiceMapForCell(Cell(board, 0, col)).SetSingleBit(1, false);
            }
            // Cells [0,7] and [0,8] keep all 9 bits — candidates 0,1 are the hidden pair,
            // bits 2-8 are the "extra" candidates that should be removed.

            var row0 = board.Groups[0];
            bool changed = solver.FindHiddenSets(row0);

            Assert.True(changed, "FindHiddenSets should detect hidden pair {0,1} in row 0");
            Assert.Equal(2, solver.GetChoiceMapForCell(Cell(board, 0, 7)).Ones);
            Assert.True(solver.GetChoiceMapForCell(Cell(board, 0, 7)).GetSingleBit(0));
            Assert.True(solver.GetChoiceMapForCell(Cell(board, 0, 7)).GetSingleBit(1));
            Assert.Equal(2, solver.GetChoiceMapForCell(Cell(board, 0, 8)).Ones);
            Assert.True(solver.GetChoiceMapForCell(Cell(board, 0, 8)).GetSingleBit(0));
            Assert.True(solver.GetChoiceMapForCell(Cell(board, 0, 8)).GetSingleBit(1));
        }

        [Fact]
        public void FindHiddenSets_NoPairPresent_ReturnsFalse()
        {
            var (solver, board) = EmptyBoard();
            // Default state: all bits set in every cell — no hidden pair exists
            // because every candidate appears in all 9 cells of each group.
            var row0 = board.Groups[0];
            bool changed = solver.FindHiddenSets(row0);
            Assert.False(changed);
        }

        // -------------------------------------------------------------------------
        // Fish (X-Wing)
        // -------------------------------------------------------------------------

        [Fact]
        public void FindFish_XWingInRows_EliminatesCandidateFromCoverColumns()
        {
            var (solver, board) = EmptyBoard();

            // X-Wing for candidate 0, using rows as base lines:
            //   Row 0: candidate 0 only in columns 2 and 5
            //   Row 3: candidate 0 only in columns 2 and 5
            // Cells [1,2] and [7,5] also have candidate 0 → should be eliminated.

            // Step 1: clear bit 0 from every cell
            foreach (CellValueSudoku cell in board.CellsMatrix)
                solver.GetChoiceMapForCell(cell).SetSingleBit(0, false);

            // Step 2: set bit 0 only in the X-Wing base cells and the two victim cells
            solver.GetChoiceMapForCell(Cell(board, 0, 2)).SetSingleBit(0, true);
            solver.GetChoiceMapForCell(Cell(board, 0, 5)).SetSingleBit(0, true);
            solver.GetChoiceMapForCell(Cell(board, 3, 2)).SetSingleBit(0, true);
            solver.GetChoiceMapForCell(Cell(board, 3, 5)).SetSingleBit(0, true);
            solver.GetChoiceMapForCell(Cell(board, 1, 2)).SetSingleBit(0, true); // victim
            solver.GetChoiceMapForCell(Cell(board, 7, 5)).SetSingleBit(0, true); // victim

            bool changed = solver.FindFish(2);

            Assert.True(changed, "FindFish should detect the X-Wing for candidate 0");

            // Victims in cover columns must lose candidate 0
            Assert.False(solver.GetChoiceMapForCell(Cell(board, 1, 2)).GetSingleBit(0),
                "[1,2] should have candidate 0 eliminated");
            Assert.False(solver.GetChoiceMapForCell(Cell(board, 7, 5)).GetSingleBit(0),
                "[7,5] should have candidate 0 eliminated");

            // Base cells must keep candidate 0
            Assert.True(solver.GetChoiceMapForCell(Cell(board, 0, 2)).GetSingleBit(0));
            Assert.True(solver.GetChoiceMapForCell(Cell(board, 0, 5)).GetSingleBit(0));
            Assert.True(solver.GetChoiceMapForCell(Cell(board, 3, 2)).GetSingleBit(0));
            Assert.True(solver.GetChoiceMapForCell(Cell(board, 3, 5)).GetSingleBit(0));
        }

        [Fact]
        public void FindFish_NoXWingPresent_ReturnsFalse()
        {
            var (solver, board) = EmptyBoard();
            // Default state: candidate 0 appears in all 9 cells of every row —
            // no row has candidate in exactly 2 positions, so no X-Wing exists.
            bool changed = solver.FindFish(2);
            Assert.False(changed);
        }
    }
}
