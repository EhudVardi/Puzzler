using System;
using System.IO;
using System.Threading.Tasks;
using Logic;
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

            bool loaded = logic.ReadFromFile(FixturePath("sudoku_9x9_easy.xml"));
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

            bool loaded = logic.ReadFromFile(FixturePath("sudoku_9x9_hard_needs_backtracking.xml"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30))) == tcs.Task;
            Assert.True(finished, "Solver did not complete within 30 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Hard Sudoku was not solved after completion");
        }

        [Fact]
        public async Task Griddler_KnownPuzzle_LoadsAndSolvesWithinTimeout()
        {
            var logic = new LogicLayerGriddler();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            logic.SolveCompleted += (_, _) => tcs.TrySetResult(true);

            bool loaded = logic.ReadFromFile(FixturePath("griddler_easy.xml"));
            Assert.True(loaded, "ReadFromFile returned false — puzzle could not be loaded");

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10))) == tcs.Task;
            Assert.True(finished, "Solver did not complete within 10 seconds");
            Assert.True(logic.RequestSolveStatus() == true, "Griddler was not solved after completion");
        }
    }
}
