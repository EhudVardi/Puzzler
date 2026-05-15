using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Logic;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class SolverBenchmarkTests
    {
        private readonly ITestOutputHelper _output;
        private static readonly TimeSpan BenchmarkTimeout = TimeSpan.FromSeconds(60);

        public SolverBenchmarkTests(ITestOutputHelper output) => _output = output;

        static string FixturePath(string name) =>
            Path.Combine(AppContext.BaseDirectory, "TestData", name);

        private async Task<(bool solved, long elapsedMs, int stepCount)> RunBenchmark(
            Action<EventHandler> subscribeStep,
            Action<EventHandler> subscribeComplete,
            Func<Task<bool>> load,
            Func<bool?> isSolved)
        {
            int stepCount = 0;
            var sw = new Stopwatch();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            subscribeStep((_, _) => stepCount++);
            subscribeComplete((_, _) => { sw.Stop(); tcs.TrySetResult(true); });

            sw.Start();
            bool loaded = await load();
            if (!loaded)
                return (false, 0, 0);

            bool finished = await Task.WhenAny(tcs.Task, Task.Delay(BenchmarkTimeout)) == tcs.Task;
            if (!finished) sw.Stop();

            return (finished && isSolved() == true, sw.ElapsedMilliseconds, stepCount);
        }

        [Theory]
        [InlineData("sudoku_9x9_easy.json",                  "9x9 Easy")]
        [InlineData("sudoku_16x16_medium.json",              "16x16 Medium")]
        [InlineData("sudoku_25x25_hard.json",              "25x25 Hard")]   // very slow — skip until backtracking is implemented
        [InlineData("sudoku_9x9_hard_needs_backtracking.json", "9x9 Hard (backtrack needed)")]
        [InlineData("sudoku_9x9_hard_s18.json",              "9x9 Hard S18")]
        public async Task Sudoku_Benchmark(string fileName, string difficulty)
        {
            var path = FixturePath(fileName);
            if (!File.Exists(path)) { _output.WriteLine($"[Sudoku/{difficulty}] SKIPPED — {fileName} not found"); return; }

            var logic = new LogicLayerSudoku();
            var (solved, ms, steps) = await RunBenchmark(
                h => logic.StepCompleted  += h,
                h => logic.SolveCompleted += h,
                () => logic.ReadFromFile(path),
                () => logic.RequestSolveStatus());

            _output.WriteLine($"[Sudoku/{difficulty}] solved={solved} | {ms} ms | {steps} steps");
            Assert.True(solved, $"[Sudoku/{difficulty}] not solved within {BenchmarkTimeout.TotalSeconds}s");
        }

        [Theory]
        [InlineData("griddler_easy.json",   "Easy")]
        [InlineData("griddler_medium.json", "Medium")]
        [InlineData("griddler_hard.json",   "Hard")]
        public async Task Griddler_Benchmark(string fileName, string difficulty)
        {
            var path = FixturePath(fileName);
            if (!File.Exists(path)) { _output.WriteLine($"[Griddler/{difficulty}] SKIPPED — {fileName} not found"); return; }

            var logic = new LogicLayerGriddler();
            var (solved, ms, steps) = await RunBenchmark(
                h => logic.StepCompleted  += h,
                h => logic.SolveCompleted += h,
                () => logic.ReadFromFile(path),
                () => logic.RequestSolveStatus());

            _output.WriteLine($"[Griddler/{difficulty}] solved={solved} | {ms} ms | {steps} steps");
            Assert.True(solved, $"[Griddler/{difficulty}] not solved within {BenchmarkTimeout.TotalSeconds}s");
        }

        [Theory]
        [InlineData("kakuru_easy.json",   "Easy")]
        [InlineData("kakuru_medium.json", "Medium")]
        [InlineData("kakuru_hard.json",   "Hard")]
        public async Task Kakuru_Benchmark(string fileName, string difficulty)
        {
            var path = FixturePath(fileName);
            if (!File.Exists(path)) { _output.WriteLine($"[Kakuru/{difficulty}] SKIPPED — {fileName} not found"); return; }

            var logic = new LogicLayerKakuru();
            var (solved, ms, steps) = await RunBenchmark(
                h => logic.StepCompleted  += h,
                h => logic.SolveCompleted += h,
                () => logic.ReadFromFile(path),
                () => logic.RequestSolveStatus());

            _output.WriteLine($"[Kakuru/{difficulty}] solved={solved} | {ms} ms | {steps} steps");
            Assert.True(solved, $"[Kakuru/{difficulty}] not solved within {BenchmarkTimeout.TotalSeconds}s");
        }

        [Theory]
        [InlineData("triddler_easy.json",   "Easy")]
        [InlineData("triddler_medium.json", "Medium")]
        [InlineData("triddler_hard.json",   "Hard")]
        public async Task Triddler_Benchmark(string fileName, string difficulty)
        {
            var path = FixturePath(fileName);
            if (!File.Exists(path)) { _output.WriteLine($"[Triddler/{difficulty}] SKIPPED — {fileName} not found"); return; }

            var logic = new LogicLayerTriddler();
            var (solved, ms, steps) = await RunBenchmark(
                h => logic.StepCompleted  += h,
                h => logic.SolveCompleted += h,
                () => logic.ReadFromFile(path),
                () => logic.RequestSolveStatus());

            _output.WriteLine($"[Triddler/{difficulty}] solved={solved} | {ms} ms | {steps} steps");
            Assert.True(solved, $"[Triddler/{difficulty}] not solved within {BenchmarkTimeout.TotalSeconds}s");
        }
    }
}
