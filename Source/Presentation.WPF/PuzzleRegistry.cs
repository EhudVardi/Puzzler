using System;
using System.Collections.Generic;
using System.Linq;
using PresentationLogic;

namespace Presentation.WPF
{
    public enum PuzzleType { Sudoku, Kakuru, Griddler, Triddler }

    public interface IPuzzleDescriptor
    {
        PuzzleType Type { get; }
        string DisplayName { get; }
        double BoardSkewAngle { get; }
        PresentationLogicBase Create();
    }

    public static class PuzzleRegistry
    {
        private sealed class Descriptor : IPuzzleDescriptor
        {
            private readonly Func<PresentationLogicBase> _factory;
            public PuzzleType Type          { get; }
            public string     DisplayName   { get; }
            public double     BoardSkewAngle { get; }

            internal Descriptor(PuzzleType type, string displayName, Func<PresentationLogicBase> factory, double boardSkewAngle = 0)
            {
                Type           = type;
                DisplayName    = displayName;
                _factory       = factory;
                BoardSkewAngle = boardSkewAngle;
            }

            public PresentationLogicBase Create() => _factory();
        }

        public static readonly IReadOnlyList<IPuzzleDescriptor> All = new IPuzzleDescriptor[]
        {
            new Descriptor(PuzzleType.Sudoku,   "Sudoku",   () => new PresentationLogicSudoku()),
            new Descriptor(PuzzleType.Kakuru,   "Kakuru",   () => new PresentationLogicKakuru()),
            new Descriptor(PuzzleType.Griddler, "Griddler", () => new PresentationLogicGriddler()),
            new Descriptor(PuzzleType.Triddler, "Triddler", () => new PresentationLogicTriddler(), boardSkewAngle: -30),
        };

        public static IPuzzleDescriptor Find(string displayName) =>
            All.First(d => d.DisplayName == displayName);
    }
}
