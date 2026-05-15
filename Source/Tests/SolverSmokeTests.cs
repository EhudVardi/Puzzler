using System;
using System.IO;
using System.Threading.Tasks;
using Logic;
using Logic.Kurodoko;
using Common.Models.Kurodoko;
using Xunit;

namespace Tests
{
    public class SolverSmokeTests
    {
        static string FixturePath(string name) =>
            Path.Combine(AppContext.BaseDirectory, "TestData", name);

        [Fact]
        public async Task Sudoku_KnownPuzzle_LoadsAndSolvesWithinTimeout()
        {
            var logic = new LogicLayerSudoku();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("sudoku_9x9_easy.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10))) == tcs.Task;
            Assert.True(finished, "Solver did not complete within 10 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Sudoku was not solved after completion");
        }

        [Fact]
        public async Task Sudoku_HardPuzzle_SolvesWithBacktracking()
        {
            var logic = new LogicLayerSudoku();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("sudoku_9x9_hard_needs_backtracking.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30))) == tcs.Task;
            Assert.True(finished, "Solver did not complete within 30 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Hard Sudoku was not solved after completion");
        }

        [Fact]
        public async Task Kakuru_HardPuzzle_SolvesWithBacktracking()
        {
            var logic = new LogicLayerKakuru();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("kakuru_hard.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30))) == tcs.Task;
            Assert.True(finished, "Kakuru hard solver did not complete within 30 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Kakuru hard was not solved after completion");
        }

        [Fact]
        public async Task Griddler_KnownPuzzle_LoadsAndSolvesWithinTimeout()
        {
            var logic = new LogicLayerGriddler();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("griddler_easy.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10))) == tcs.Task;
            Assert.True(finished, "Solver did not complete within 10 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Griddler was not solved after completion");
        }

        [Fact]
        public async Task Griddler_HardPuzzle_SolvesWithBacktracking()
        {
            var logic = new LogicLayerGriddler();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("griddler_hard.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(60))) == tcs.Task;
            Assert.True(finished, "Griddler hard solver did not complete within 60 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Griddler hard was not solved after completion");
        }

        [Fact]
        public async Task Triddler_HardPuzzle_SolvesWithBacktracking()
        {
            var logic = new LogicLayerTriddler();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("triddler_hard.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(60))) == tcs.Task;
            Assert.True(finished, "Triddler hard solver did not complete within 60 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Triddler hard was not solved after completion");
        }

        [Fact]
        public async Task Triddler_23x23Hard_SolvesWithinTimeout()
        {
            var logic = new LogicLayerTriddler();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("triddler_23x23_hard.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10))) == tcs.Task;
            Assert.True(finished, "Triddler 23x23 hard solver did not complete within 10 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Triddler 23x23 hard was not solved after completion");
        }

        [Fact]
        public async Task Triddler_KitchenKnife25x25_SolvesWithinTimeout()
        {
            var logic = new LogicLayerTriddler();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("triddler_kitchen_knife_25x25.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromMinutes(3))) == tcs.Task;
            Assert.True(finished, "Triddler Kitchen Knife solver did not complete within 3 minutes");
            Assert.True(logic.RequestSolveStatus() == true, "Kitchen Knife was not solved after completion");
        }

        [Fact]
        public async Task Kurodoko_Easy_SolvesWithinTimeout()
        {
            var logic = new LogicLayerKurodoko();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("kurodoko_easy.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10))) == tcs.Task;
            Assert.True(finished, "Kurodoko easy solver did not complete within 10 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Kurodoko easy was not solved after completion");
        }

        [Fact]
        public async Task Kurodoko_Hard_SolvesWithinTimeout()
        {
            var logic = new LogicLayerKurodoko();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = await logic.ReadFromFile(FixturePath("kurodoko_hard.json"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(60))) == tcs.Task;
            Assert.True(finished, "Kurodoko hard solver did not complete within 60 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Kurodoko hard was not solved after completion");
        }

        [Fact]
        public void Kurodoko_Generator_ProducesSparseSolvablePuzzle()
        {
            var factory = new FactoryKurodoko();
            var board   = factory.GenerateRandom(); // 19×11, TimeBoundedMin, 10s cap

            // Must have at least one clue but not all whites carrying a clue
            Assert.True(board.InitialCells.Count > 0, "Generator produced no clues");

            // After solving, there must be white cells that had no clue (sparse check)
            var solver = new SolverKurodoko { Board = board };
            solver.SolveBoard();
            Assert.True(solver.IsSolved(), "Generated puzzle is not solvable");

            int whiteFree = 0;
            foreach (var cell in board.ValueCells)
                if (cell.Value == false && !cell.IsFixed)
                    whiteFree++;
            Assert.True(whiteFree > 0, "Every white cell has a clue — puzzle is not sparse");
        }
    }
}
