using System;
using Logic;
using Data.DataModels;
using Common.Models.Sudoku;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicSudoku : PresentationLogicGeneric<SudokuPuzzle, BoardSudoku>
    {
        public PresentationLogicSudoku()
        {
            this.LogicProxy = new LogicLayerSudoku();
            this.URL = "http://www.sudokuconquest.com/9x9/expert";
        }

        public override void DrawBoard(BoardSudoku trackerBoard, BoardSudoku solvedBoard, float width, float height)
        {
            float cellWidth  = width  / trackerBoard.Columns;
            float cellHeight = height / trackerBoard.Rows;

            float widthB  = width  / trackerBoard.N;
            float heightB = height / trackerBoard.M;

            // --- watermark: active number faintly over the whole board ---
            var watermarkColor = bText.WithAlpha(28);
            DrawText((activeNumber + 1).ToString(), fontBold, watermarkColor, 0, 0, width, height);

            // --- cell backgrounds ---
            foreach (CellValueSudoku valueCell in trackerBoard.ValueCells)
            {
                bool matchesActive = valueCell.Value.HasValue && valueCell.Value == activeNumber;

                PuzzlerColor backColor;
                if (matchesActive)
                    backColor = PuzzlerColor.Wheat;
                else if (!trackerBoard.InitialCells.Contains(valueCell))
                    backColor = bNull;
                else
                    backColor = bFixed;

                FillRect(backColor,
                    cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);
            }

            // --- grid lines and box borders ---
            for (int i = 0; i < trackerBoard.CellsMatrix.GetLength(0); i++)
                DrawLine(PuzzlerColor.Black, 1, 0, cellHeight * i, width, cellHeight * i);

            for (int j = 0; j < trackerBoard.CellsMatrix.GetLength(1); j++)
                DrawLine(PuzzlerColor.Black, 1, cellWidth * j, 0, cellWidth * j, height);

            float marginBoxes = margin / 4;
            for (int i = 0; i < trackerBoard.N; i++)
                for (int j = 0; j < trackerBoard.M; j++)
                    DrawRect(PuzzlerColor.Black, 1,
                        widthB * j + marginBoxes, heightB * i + marginBoxes,
                        widthB - marginBoxes * 2f, heightB - marginBoxes * 2f);

            // --- cell digits and hints ---
            int hintRows = trackerBoard.N;   // sub-grid rows per cell
            int hintCols = trackerBoard.M;   // sub-grid cols per cell

            foreach (CellValueSudoku valueCell in trackerBoard.ValueCells)
            {
                CellValueSudoku? solvedValueCell = solvedBoard.CellsMatrix[valueCell.Row, valueCell.Column] as CellValueSudoku;
                bool isFixed = trackerBoard.InitialCells.Contains(valueCell);
                PuzzlerColor foreColor = isFixed ? bText : bCorrect;

                float cx = cellWidth  * valueCell.Column + margin;
                float cy = cellHeight * valueCell.Row    + margin;
                float cw = cellWidth  - margin * 2f;
                float ch = cellHeight - margin * 2f;

                switch (this.displayType)
                {
                    case DisplayType.Board:
                    case DisplayType.Hint:
                        if (valueCell.Value.HasValue)
                        {
                            PuzzlerColor textColor = foreColor;
                            if (this.displayType == DisplayType.Hint && isFixed
                                && selectedValueCell?.Value.HasValue == true
                                && selectedValueCell.Value == valueCell.Value)
                                textColor = bIncorrect;

                            DrawText((valueCell.Value + 1).ToString() ?? "", font, textColor, cx, cy, cw, ch);
                        }
                        else if (!isFixed && valueCell.Hints.Count > 0)
                        {
                            float subW = cw / hintCols;
                            float subH = ch / hintRows;
                            foreach (int k in valueCell.Hints)
                            {
                                int sr = k / hintCols;
                                int sc = k % hintCols;
                                float sx = cx + sc * subW;
                                float sy = cy + sr * subH;
                                PuzzlerColor hintColor = (k == activeNumber) ? bCorrect : PuzzlerColor.Gray;
                                DrawText((k + 1).ToString(), font, hintColor, sx, sy, subW, subH);
                            }
                        }
                        break;

                    case DisplayType.Solution:
                        DrawText((solvedValueCell!.Value + 1).ToString() ?? "", font, foreColor, cx, cy, cw, ch);
                        break;
                }
            }

            // --- selected cell border (top-most layer) ---
            if (selectedValueCell != null)
                DrawRect(PuzzlerColor.Black, margin,
                    cellWidth * selectedValueCell.Column + margin, cellHeight * selectedValueCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);
        }

        public override (int Width, int Height) GetPrefferedSize()
        {
            return (40 * (GetTrackerBoard()?.Columns ?? 9), 40 * (GetTrackerBoard()?.Rows ?? 9));
        }

        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            BoardSudoku? b = this.GetTrackerBoard();
            if (b == null) return;
            int column = (int)(e.X / (sizeX / b.Columns));
            int row    = (int)(e.Y / (sizeY / b.Rows));
            if (row < 0 || row >= b.Rows || column < 0 || column >= b.Columns) return;

            if (b.CellsMatrix[row, column] is not CellValueSudoku pointedCell) return;
            if (b.InitialCells.Contains(pointedCell)) return;

            if (!ReferenceEquals(pointedCell, selectedValueCell))
            {
                selectedValueCell = pointedCell;
            }
            else if (e.Button == PointerButton.Left)
            {
                if (pointedCell.Value == activeNumber) pointedCell.Value = null;
                else                                   pointedCell.Value = activeNumber;
            }
            else if (e.Button == PointerButton.Right)
            {
                if (!pointedCell.Hints.Add(activeNumber))
                    pointedCell.Hints.Remove(activeNumber);
            }

            this.OnRequestRefresh(EventArgs.Empty);
        }

        public override void HandlePointerWheel(PointerEvent e, float sizeX, float sizeY)
        {
            BoardSudoku? b = GetTrackerBoard();
            if (b == null) return;
            int max  = b.Size;
            int next = (activeNumber + (e.Delta > 0 ? 1 : -1)) % max;
            activeNumber = next < 0 ? next + max : next;
            this.OnRequestRefresh(EventArgs.Empty);
        }

        private CellValueSudoku? selectedValueCell;
        private int activeNumber;
    }
}
