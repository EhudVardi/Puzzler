using System;
using Common.Models.Kurodoko;
using Data.DataModels;
using Logic;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicKurodoko : PresentationLogicGeneric<KurodokoPuzzle, BoardKurodoko>
    {
        public PresentationLogicKurodoko()
        {
            this.LogicProxy = new LogicLayerKurodoko();
        }

        // ── colours ───────────────────────────────────────────────────────────
        private static readonly PuzzlerColor cBlack      = PuzzlerColor.FromArgb(255,  30,  30,  30);
        private static readonly PuzzlerColor cWhite      = PuzzlerColor.Snow;
        private static readonly PuzzlerColor cClue       = PuzzlerColor.FromArgb(255,  70,  70,  90);
        private static readonly PuzzlerColor cClueText   = PuzzlerColor.White;
        private static readonly PuzzlerColor cDot        = PuzzlerColor.FromArgb(255, 120, 120, 160);
        private static readonly PuzzlerColor cGrid       = PuzzlerColor.FromArgb(255, 100, 100, 120);
        private static readonly PuzzlerColor cSelection  = PuzzlerColor.FromArgb(200, 255, 200,  50);
        private static readonly PuzzlerColor cError      = PuzzlerColor.Red;

        private CellValueKurodoko? _selectedCell;

        // ── render ────────────────────────────────────────────────────────────

        public override void DrawBoard(BoardKurodoko trackerBoard, BoardKurodoko solvedBoard,
                                        float width, float height)
        {
            float cellW = width  / trackerBoard.Columns;
            float cellH = height / trackerBoard.Rows;

            for (int r = 0; r < trackerBoard.Rows; r++)
            {
                for (int c = 0; c < trackerBoard.Columns; c++)
                {
                    float x = c * cellW;
                    float y = r * cellH;

                    var cell   = trackerBoard.Cell(r, c);
                    var solved = solvedBoard.Cell(r, c);

                    DrawCell(cell, solved, x, y, cellW, cellH);
                }
            }

            // Grid lines
            for (int r = 0; r <= trackerBoard.Rows; r++)
                DrawLine(cGrid, 1, 0, r * cellH, width, r * cellH);
            for (int c = 0; c <= trackerBoard.Columns; c++)
                DrawLine(cGrid, 1, c * cellW, 0, c * cellW, height);

            // Selection cursor
            if (_selectedCell != null)
            {
                float sx = _selectedCell.Column * cellW + margin;
                float sy = _selectedCell.Row    * cellH + margin;
                DrawRect(cSelection, margin * 2, sx, sy, cellW - margin * 2, cellH - margin * 2);
            }
        }

        private void DrawCell(CellValueKurodoko cell, CellValueKurodoko solved,
                               float x, float y, float w, float h)
        {
            bool? displayValue = displayType == DisplayType.Solution ? solved.Value : cell.Value;

            if (cell.ClueNumber.HasValue)
            {
                // Clue cell — always dark background with number
                FillRect(cClue, x + 1, y + 1, w - 2, h - 2);
                DrawText(cell.ClueNumber.Value.ToString(), fontBold, cClueText,
                    x + 2, y + 2, w - 4, h - 4);
            }
            else if (displayValue == true)
            {
                // Black cell
                FillRect(cBlack, x + 1, y + 1, w - 2, h - 2);

                // In Hint mode highlight errors (cell is black but solution is white, or vice versa)
                if (displayType == DisplayType.Hint && solved.Value != true)
                    FillRect(cError.WithAlpha(100), x + 1, y + 1, w - 2, h - 2);
            }
            else if (displayValue == false)
            {
                // White cell — with dot marker to show player has decided it's white
                FillRect(cWhite, x + 1, y + 1, w - 2, h - 2);
                float dotR = Math.Min(w, h) * 0.08f;
                float cx   = x + w / 2;
                float cy   = y + h / 2;
                FillRect(cDot, cx - dotR, cy - dotR, dotR * 2, dotR * 2);

                if (displayType == DisplayType.Hint && solved.Value != false)
                    FillRect(cError.WithAlpha(100), x + 1, y + 1, w - 2, h - 2);
            }
            else
            {
                // Undecided — plain white
                FillRect(cWhite, x + 1, y + 1, w - 2, h - 2);
            }
        }

        // ── preferred size ────────────────────────────────────────────────────

        public override (int Width, int Height) GetPrefferedSize()
        {
            var b = GetTrackerBoard();
            if (b == null) return (500, 500);
            return (Math.Max(b.Columns * 60, 300), Math.Max(b.Rows * 60, 300));
        }

        // ── interaction ───────────────────────────────────────────────────────

        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            var board = GetTrackerBoard();
            if (board == null) return;

            float cellW = sizeX / board.Columns;
            float cellH = sizeY / board.Rows;

            int col = (int)(e.X / cellW);
            int row = (int)(e.Y / cellH);

            if (row < 0 || row >= board.Rows || col < 0 || col >= board.Columns) return;

            var cell = board.Cell(row, col);
            if (cell.IsFixed) return; // clue cells cannot be modified

            if (!ReferenceEquals(cell, _selectedCell))
            {
                _selectedCell = cell;
            }
            else if (e.Button == PointerButton.Left)
            {
                // Left-click cycles: null → black → null
                cell.Value = cell.Value == null ? true : null;
            }
            else if (e.Button == PointerButton.Right)
            {
                // Right-click cycles: null → white-marked → null
                cell.Value = cell.Value == null ? false : null;
            }

            OnRequestRefresh(EventArgs.Empty);
        }

        public override void HandleKey(KeyEvent e)
        {
            var board = GetTrackerBoard();
            if (board == null || _selectedCell == null) return;

            int vk  = e.KeyValue;
            int row = _selectedCell.Row;
            int col = _selectedCell.Column;

            // Arrow keys — move selection
            if (vk == 0x26 && row > 0)             { _selectedCell = board.Cell(row - 1, col); OnRequestRefresh(EventArgs.Empty); return; }
            if (vk == 0x28 && row < board.Rows - 1) { _selectedCell = board.Cell(row + 1, col); OnRequestRefresh(EventArgs.Empty); return; }
            if (vk == 0x25 && col > 0)              { _selectedCell = board.Cell(row, col - 1); OnRequestRefresh(EventArgs.Empty); return; }
            if (vk == 0x27 && col < board.Columns - 1) { _selectedCell = board.Cell(row, col + 1); OnRequestRefresh(EventArgs.Empty); return; }

            if (_selectedCell.IsFixed) return;

            // Enter = toggle black (like left-click)
            if (vk == 0x0D)
            {
                _selectedCell.Value = _selectedCell.Value == null ? true : null;
                OnRequestRefresh(EventArgs.Empty);
            }
            // Space = toggle white marker (like right-click)
            else if (vk == 0x20)
            {
                _selectedCell.Value = _selectedCell.Value == null ? false : null;
                OnRequestRefresh(EventArgs.Empty);
            }
        }
    }
}
