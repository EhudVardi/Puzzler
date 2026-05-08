using System;
using System.Collections.Generic;
using Common.Models.Base;
using Common.Models.Yakugo;
using Data.DataModels;

namespace Logic
{
    public class FactoryYakugo : FactoryGeneric<PuzzleYakugo, BoardYakugo>
    {
        protected override BoardYakugo? CreateBoardFromPuzzleObject(PuzzleYakugo puzzle)
        {
            var board = new BoardYakugo
            {
                SourceLanguage = puzzle.SourceLanguage,
                TargetLanguage = puzzle.TargetLanguage,
            };

            var matrix = new CellBase[puzzle.Rows, puzzle.Cols];
            board.CellsMatrix = matrix;

            // First pass: create all cells
            foreach (var cd in puzzle.Cells)
            {
                if (cd.Kind == "Clue")
                {
                    var holder = new CellGroupHolderYakugo(cd.Row, cd.Col);
                    matrix[cd.Row, cd.Col] = holder;
                }
                else // Letter
                {
                    var cell = new CellValueYakugo(cd.Row, cd.Col);
                    if (cd.Initial != null && cd.Initial.Length > 0)
                    {
                        cell.Value = cd.Initial[0];
                        cell.IsInitialHint = true;
                    }
                    matrix[cd.Row, cd.Col] = cell;
                }
            }

            // Fill remaining cells as null (empty / outside board) — CellBase array already null

            // Second pass: build groups (one per clue entry) and link cells
            foreach (var cd in puzzle.Cells)
            {
                if (cd.Kind != "Clue" || cd.Clues == null) continue;
                var holder = (CellGroupHolderYakugo)matrix[cd.Row, cd.Col];

                foreach (var clueData in cd.Clues)
                {
                    if (!Enum.TryParse<YGDirection>(clueData.Dir, out var dir))
                        dir = YGDirection.Right;

                    string targetLetters = StripSpaces(clueData.Target);

                    var group = new GroupYakugo
                    {
                        SourceText    = clueData.Source,
                        TargetText    = clueData.Target,
                        LengthPattern = clueData.Pattern,
                        OriginRow     = cd.Row,
                        OriginCol     = cd.Col,
                        Direction     = dir,
                    };

                    var (dRow, dCol) = dir.Delta();
                    int r = cd.Row + dRow;
                    int c = cd.Col + dCol;

                    for (int i = 0; i < targetLetters.Length; i++)
                    {
                        if (r < 0 || r >= puzzle.Rows || c < 0 || c >= puzzle.Cols)
                            break;

                        if (matrix[r, c] is CellValueYakugo letter)
                        {
                            group.Cells.Add(letter);
                            letter.Groups.Add(group);
                        }

                        r += dRow;
                        c += dCol;
                    }

                    holder.Clues.Add(group);
                    board.Groups.Add(group);

                    // Track initial cells
                    foreach (var cell in group.Cells)
                        if (cell.IsInitialHint && !board.InitialCells.Contains(cell))
                            board.InitialCells.Add(cell);
                }
            }

            return board;
        }

        protected override PuzzleYakugo? CreatePuzzleObjectFromBoard(BoardYakugo board)
        {
            var puzzle = new PuzzleYakugo
            {
                Rows           = board.Rows,
                Cols           = board.Columns,
                SourceLanguage = board.SourceLanguage,
                TargetLanguage = board.TargetLanguage,
            };

            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Columns; c++)
                {
                    var cell = board.CellsMatrix[r, c];
                    if (cell == null) continue;

                    if (cell is CellGroupHolderYakugo holder)
                    {
                        var cd = new PuzzleCellYG { Row = r, Col = c, Kind = "Clue", Clues = new List<PuzzleClueYG>() };
                        foreach (var g in holder.Clues)
                            cd.Clues.Add(new PuzzleClueYG
                            {
                                Source  = g.SourceText,
                                Target  = g.TargetText,
                                Dir     = g.Direction.ToString(),
                                Pattern = g.LengthPattern,
                            });
                        puzzle.Cells.Add(cd);
                    }
                    else if (cell is CellValueYakugo letter)
                    {
                        puzzle.Cells.Add(new PuzzleCellYG
                        {
                            Row     = r,
                            Col     = c,
                            Kind    = "Letter",
                            Initial = letter.IsInitialHint && letter.Value.HasValue ? letter.Value.Value.ToString() : null,
                        });
                    }
                }
            }

            return puzzle;
        }

        private static string StripSpaces(string s)
        {
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (char ch in s)
                if (ch != ' ' && ch != '-')
                    sb.Append(ch);
            return sb.ToString();
        }
    }
}
