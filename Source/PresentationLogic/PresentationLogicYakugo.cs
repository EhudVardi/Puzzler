using System;
using System.Collections.Generic;
using System.Linq;
using Common.Models.Base;
using Common.Models.Yakugo;
using Data.DataModels;
using Logic;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicYakugo
        : PresentationLogicGeneric<PuzzleYakugo, BoardYakugo>
    {
        public PresentationLogicYakugo()
        {
            this.LogicProxy = new LogicLayerYakugo();
        }

        // ── selection state ──────────────────────────────────────────────────
        private GroupYakugo? _activeGroup;
        private int _activeCellIndex = 0;   // index within _activeGroup.Cells

        // ── colours ──────────────────────────────────────────────────────────
        private static readonly PuzzlerColor cClueBg        = PuzzlerColor.FromArgb(255,  55,  55,  75);
        private static readonly PuzzlerColor cClueBgSolved  = PuzzlerColor.FromArgb(255,  30,  80,  50);
        private static readonly PuzzlerColor cClueBorder    = PuzzlerColor.FromArgb(255, 110, 110, 140);
        private static readonly PuzzlerColor cLetterBg      = PuzzlerColor.FromArgb(255,  40,  40,  55);
        private static readonly PuzzlerColor cLetterHint    = PuzzlerColor.FromArgb(255,  35,  60,  80);
        private static readonly PuzzlerColor cLetterFg      = PuzzlerColor.FromArgb(255, 220, 220, 240);
        private static readonly PuzzlerColor cHintFg        = PuzzlerColor.FromArgb(255, 100, 200, 160);
        private static readonly PuzzlerColor cTrack         = PuzzlerColor.FromArgb(80,  120, 180, 255);
        private static readonly PuzzlerColor cActiveCursor  = PuzzlerColor.FromArgb(200, 120, 180, 255);
        private static readonly PuzzlerColor cCheckMark     = PuzzlerColor.FromArgb(255,  80, 210, 120);
        private static readonly PuzzlerColor cHeaderBg      = PuzzlerColor.FromArgb(255,  28,  28,  42);
        private static readonly PuzzlerColor cProgressArc   = PuzzlerColor.FromArgb(255,  90, 160, 255);
        private static readonly PuzzlerColor cSourceText    = PuzzlerColor.FromArgb(255, 200, 200, 220);
        private static readonly PuzzlerColor cArrow         = PuzzlerColor.FromArgb(255, 140, 140, 170);
        private static readonly PuzzlerColor cDivider       = PuzzlerColor.FromArgb(120, 140, 140, 170);
        private static readonly PuzzlerColor cEmpty         = PuzzlerColor.FromArgb(255,  28,  28,  42);
        private static readonly PuzzlerColor cBoardBorder   = PuzzlerColor.FromArgb(255,  70,  70,  95);
        private static readonly PuzzlerColor cMissLetter    = PuzzlerColor.FromArgb(255, 200,  80,  80);
        private static readonly PuzzlerColor cDirectionStrip = PuzzlerColor.FromArgb(255,  28,  28,  42);

        private const float HeaderH = 36f;

        // ── render ───────────────────────────────────────────────────────────
        public override void DrawBoard(BoardYakugo trackerBoard, BoardYakugo solvedBoard,
                                       float width, float height)
        {
            float boardH = height - HeaderH;
            float cellW  = width  / trackerBoard.Columns;
            float cellH  = boardH / trackerBoard.Rows;

            DrawHeader(trackerBoard, solvedBoard, width);

            // Highlight active group track
            if (_activeGroup != null)
            {
                foreach (var cell in _activeGroup.Cells)
                    FillRect(cTrack,
                        cell.Column * cellW, HeaderH + cell.Row * cellH, cellW, cellH);

                // cursor cell
                if (_activeCellIndex >= 0 && _activeCellIndex < _activeGroup.Cells.Count)
                {
                    var cur = _activeGroup.Cells[_activeCellIndex];
                    FillRect(cActiveCursor,
                        cur.Column * cellW, HeaderH + cur.Row * cellH, cellW, cellH);
                }
            }

            // Draw all cells
            for (int r = 0; r < trackerBoard.Rows; r++)
            {
                for (int c = 0; c < trackerBoard.Columns; c++)
                {
                    float x = c * cellW;
                    float y = HeaderH + r * cellH;
                    var rawCell = trackerBoard.CellsMatrix[r, c];

                    if (rawCell == null)
                    {
                        FillRect(cEmpty, x, y, cellW, cellH);
                        continue;
                    }

                    if (rawCell is CellGroupHolderYakugo holder)
                        DrawClueCell(holder, x, y, cellW, cellH);
                    else if (rawCell is CellValueYakugo letter)
                        DrawLetterCell(letter, trackerBoard, x, y, cellW, cellH);
                }
            }

            // Grid border lines
            for (int r = 0; r <= trackerBoard.Rows; r++)
                DrawLine(cBoardBorder, 1, 0, HeaderH + r * cellH, width, HeaderH + r * cellH);
            for (int c = 0; c <= trackerBoard.Columns; c++)
                DrawLine(cBoardBorder, 1, c * cellW, HeaderH, c * cellW, height);
        }

        private const float StripW   = 10f;
        private const float MaxTextH = 26f;

        private void DrawClueCell(CellGroupHolderYakugo holder,
                                   float x, float y, float w, float h)
        {
            int n = holder.Clues.Count;
            float slotH = h / n;

            for (int i = 0; i < n; i++)
            {
                var g = holder.Clues[i];
                float sy = y + i * slotH;

                bool isActive  = ReferenceEquals(g, _activeGroup);
                bool isSolved  = g.IsSolved;
                PuzzlerColor bg = isSolved ? cClueBgSolved : cClueBg;
                if (isActive) bg = bg.WithAlpha(220);

                FillRect(bg, x + 1, sy + 1, w - 2, slotH - 2);

                // Divider between stacked clues
                if (i > 0)
                    DrawLine(cDivider, 1, x, sy, x + w, sy);

                // Direction strip — dark band at the edge the word extends toward
                DrawDirectionStrip(g.Direction, x, sy, w, slotH);

                const float padX = 4f;
                float textH = Math.Min(slotH * 0.55f, MaxTextH);
                float textW = w - padX * 2;
                float textY = sy + (slotH - textH) * 0.5f;

                string label = g.SourceText;
                if (g.LengthPattern != null)
                    label += $" ({string.Join(",", g.LengthPattern)})";
                DrawText(label, font, cSourceText, x + padX, textY, textW, textH);

                if (isSolved)
                    DrawText("✓", font, cCheckMark,
                        x + w - 20f, sy + 2f, 18f, 18f);
            }

            DrawRect(cClueBorder, 1, x, y, w, h);
        }

        private void DrawDirectionStrip(YGDirection direction, float x, float y, float w, float h)
        {
            switch (direction)
            {
                case YGDirection.Down:
                    FillRect(cDirectionStrip, x + 1, y + h - StripW, w - 2, StripW - 1);
                    break;
                case YGDirection.Up:
                    FillRect(cDirectionStrip, x + 1, y + 1, w - 2, StripW);
                    break;
                case YGDirection.Left:
                    FillRect(cDirectionStrip, x + 1, y + 1, StripW, h - 2);
                    break;
                case YGDirection.Right:
                    FillRect(cDirectionStrip, x + w - StripW - 1, y + 1, StripW, h - 2);
                    break;
            }
        }

        private void DrawLetterCell(CellValueYakugo cell, BoardYakugo board,
                                     float x, float y, float w, float h)
        {
            PuzzlerColor bg = cell.IsInitialHint ? cLetterHint : cLetterBg;
            FillRect(bg, x, y, w, h);

            char? displayChar = null;
            PuzzlerColor fg = cLetterFg;

            switch (displayType)
            {
                case DisplayType.Board:
                case DisplayType.Hint:
                    displayChar = cell.Value;
                    fg = cell.IsInitialHint ? cHintFg : cLetterFg;

                    // Conflict highlight: cell filled but doesn't match a group's target
                    if (cell.Value.HasValue && !cell.IsInitialHint)
                    {
                        bool conflict = false;
                        foreach (var g in cell.Groups)
                        {
                            int idx = g.Cells.IndexOf(cell);
                            if (idx >= 0 && idx < g.TargetLetters.Length &&
                                cell.Value.Value != g.TargetLetters[idx])
                            { conflict = true; break; }
                        }
                        if (conflict) fg = cMissLetter;
                    }
                    break;

                case DisplayType.Solution:
                    // Show solved-board letter
                    if (board.CellsMatrix[cell.Row, cell.Column] is CellValueYakugo sv)
                        displayChar = sv.Value;
                    break;
            }

            if (displayChar.HasValue)
                DrawText(displayChar.Value.ToString(), fontBold, fg,
                    x + 2, y + 2, w - 4, h - 4);

            // Hint check-mark badge
            if (cell.IsInitialHint)
                DrawText("✓", font, cCheckMark,
                    x + w * 0.55f, y + h * 0.55f, w * 0.4f, h * 0.4f);
        }

        private void DrawHeader(BoardYakugo board, BoardYakugo solvedBoard,
                                 float width)
        {
            FillRect(cHeaderBg, 0, 0, width, HeaderH);
            DrawLine(cBoardBorder, 1, 0, HeaderH, width, HeaderH);

            // Progress fraction: "20/44"
            string progress = $"{board.FilledCount}/{board.TotalCount}";
            DrawText(progress, font, cProgressArc, width - 70, 4, 66, HeaderH - 8);

            if (_activeGroup == null) return;

            // "_ _ _ : sourceText (len)"  — blank-slots pattern
            string slots = BuildSlotPattern(_activeGroup);
            string headerText = $"{slots} : {_activeGroup.SourceText}";
            if (_activeGroup.LengthPattern != null)
                headerText += $" ({string.Join(",", _activeGroup.LengthPattern)})";

            DrawText(headerText, fontBold, cLetterFg, 8, 4, width - 90, HeaderH - 8);
        }

        private string BuildSlotPattern(GroupYakugo group)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < group.Cells.Count; i++)
            {
                if (i > 0) sb.Append(' ');
                char? v = group.Cells[i].Value;
                sb.Append(v.HasValue ? v.Value.ToString() : "_");
            }
            return sb.ToString();
        }

        // ── preferred size ───────────────────────────────────────────────────
        public override (int Width, int Height) GetPrefferedSize()
        {
            var b = GetTrackerBoard();
            if (b == null) return (600, 500);
            int w = Math.Max(b.Columns * 70, 300);
            int h = Math.Max(b.Rows    * 50, 200) + (int)HeaderH;
            return (w, h);
        }

        // ── interaction ──────────────────────────────────────────────────────
        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            var board = GetTrackerBoard();
            if (board == null) return;

            float boardH = sizeY - HeaderH;
            float cellW  = sizeX / board.Columns;
            float cellH  = boardH / board.Rows;

            int col = (int)(e.X / cellW);
            int row = (int)((e.Y - HeaderH) / cellH);

            if (row < 0 || row >= board.Rows || col < 0 || col >= board.Columns)
            {
                OnRequestRefresh(EventArgs.Empty);
                return;
            }

            var rawCell = board.CellsMatrix[row, col];

            if (rawCell is CellGroupHolderYakugo holder && holder.Clues.Count > 0)
            {
                // Click on a clue cell: pick sub-slot, cycle if clicking same cell
                int slotIndex = 0;
                if (holder.Clues.Count == 2)
                {
                    float relY = (e.Y - HeaderH) - row * cellH;
                    slotIndex = relY > cellH / 2f ? 1 : 0;
                }

                var targetGroup = holder.Clues[slotIndex];

                if (ReferenceEquals(targetGroup, _activeGroup) && holder.Clues.Count > 1)
                    _activeGroup = holder.Clues[1 - slotIndex]; // cycle
                else
                    _activeGroup = targetGroup;

                _activeCellIndex = 0;
            }
            else if (rawCell is CellValueYakugo letter)
            {
                // Click on a letter cell: select the group that passes through it
                // If multiple groups pass through, cycle with repeated clicks
                if (letter.Groups.Count > 0)
                {
                    int currentIdx = _activeGroup == null ? -1
                        : letter.Groups.IndexOf(_activeGroup);
                    int next = (currentIdx + 1) % letter.Groups.Count;
                    _activeGroup = letter.Groups[next];
                    _activeCellIndex = _activeGroup.Cells.IndexOf(letter);
                }
            }

            OnRequestRefresh(EventArgs.Empty);
        }

        public override void HandleKey(KeyEvent e)
        {
            var board = GetTrackerBoard();
            if (board == null || _activeGroup == null) return;

            int vk = e.KeyValue;

            // Arrow keys — advance/retreat cursor along active vector
            if (IsAdvanceKey(vk))  { AdvanceCursor(+1); OnRequestRefresh(EventArgs.Empty); return; }
            if (IsRetreatKey(vk))  { AdvanceCursor(-1); OnRequestRefresh(EventArgs.Empty); return; }

            // Backspace / Delete — erase current cell and retreat
            if (vk == 0x08 || vk == 0x2E)   // VK_BACK | VK_DELETE
            {
                SetCurrentCell(null);
                if (vk == 0x08) AdvanceCursor(-1);
                OnRequestRefresh(EventArgs.Empty);
                return;
            }

            // All printable characters are handled exclusively by HandleTextInput,
            // which fires after KeyDown for any keystroke that produces text and works
            // correctly for both Latin and Hebrew keyboard layouts without double-firing.
        }

        private bool IsAdvanceKey(int vk)
        {
            if (_activeGroup == null) return false;
            return _activeGroup.Direction switch
            {
                YGDirection.Right => vk == 0x27, // VK_RIGHT
                YGDirection.Left  => vk == 0x25, // VK_LEFT
                YGDirection.Down  => vk == 0x28, // VK_DOWN
                YGDirection.Up    => vk == 0x26, // VK_UP
                _ => false,
            };
        }

        private bool IsRetreatKey(int vk)
        {
            if (_activeGroup == null) return false;
            return _activeGroup.Direction switch
            {
                YGDirection.Right => vk == 0x25,
                YGDirection.Left  => vk == 0x27,
                YGDirection.Down  => vk == 0x26,
                YGDirection.Up    => vk == 0x28,
                _ => false,
            };
        }

        private void AdvanceCursor(int delta)
        {
            if (_activeGroup == null) return;
            _activeCellIndex = Math.Clamp(
                _activeCellIndex + delta, 0, _activeGroup.Cells.Count - 1);
        }

        private void SetCurrentCell(char? value)
        {
            if (_activeGroup == null) return;
            if (_activeCellIndex < 0 || _activeCellIndex >= _activeGroup.Cells.Count) return;
            var cell = _activeGroup.Cells[_activeCellIndex];
            if (cell.IsInitialHint) return;
            cell.Value = value;
        }

        // Called from PresentationLogicBase when WPF TextInput fires
        public void HandleTextInput(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            char ch = text[0];
            if (ch < 0x20) return; // ignore control chars

            SetCurrentCell(ch);
            AdvanceCursor(+1);
            OnRequestRefresh(EventArgs.Empty);
        }
    }
}
