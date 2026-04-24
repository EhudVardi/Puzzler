using System;
using Logic;
using Data.DataModels;
using Common.Models.Kakuru;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicKakuru : PresentationLogicGeneric<PuzzleKakuru, BoardKakuru>
    {
        public PresentationLogicKakuru()
        {
            this.LogicProxy = new LogicLayerKakuru();
            this.URL = "http://www.kakuroconquest.com/9x11/expert";
        }

        public override void DrawBoard(BoardKakuru trackerBoard, BoardKakuru solvedBoard, float width, float height)
        {
            float cellWidth  = width  / trackerBoard.Columns;
            float cellHeight = height / trackerBoard.Rows;

            foreach (CellValueKakuru valueCell in trackerBoard.ValueCells)
            {
                CellValueKakuru solvedValueCell = solvedBoard.CellsMatrix[valueCell.Row, valueCell.Column] as CellValueKakuru;

                PuzzlerColor backColor = trackerBoard.InitialCells.Contains(valueCell) ? bFixed : bNull;

                FillRect(backColor,
                    cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);

                switch (this.displayType)
                {
                    case DisplayType.Board:
                        if (valueCell.IsFixed)
                            DrawText(valueCell.Value.ToString(), font, bText,
                                cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                                cellWidth - margin * 2f, cellHeight - margin * 2f);
                        break;
                    case DisplayType.Hint:
                        if (valueCell.IsFixed)
                        {
                            PuzzlerColor textColor = solvedValueCell.Value != valueCell.Value ? bIncorrect : bCorrect;
                            DrawText(valueCell.Value.ToString(), font, textColor,
                                cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                                cellWidth - margin * 2f, cellHeight - margin * 2f);
                        }
                        break;
                    case DisplayType.Solution:
                        DrawText(solvedValueCell.Value.ToString(), font, bCorrect,
                            cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                            cellWidth - margin * 2f, cellHeight - margin * 2f);
                        break;
                }
                if (valueCell.IsFixed)
                {
                    PuzzlerColor textColor2 = solvedValueCell.Value != valueCell.Value ? bIncorrect : bCorrect;
                    DrawText(valueCell.Value.ToString(), font, textColor2,
                        cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                        cellWidth - margin * 2f, cellHeight - margin * 2f);
                }
            }

            foreach (CellGroupHolderKakuru groupCell in trackerBoard.GroupHolderCells)
            {
                FillRect(bGroupHolder,
                    cellWidth * groupCell.Column + margin, cellHeight * groupCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);

                DrawLine(PuzzlerColor.Black, margin,
                    cellWidth * groupCell.Column + margin, cellHeight * groupCell.Row + margin,
                    cellWidth * (groupCell.Column + 1) - margin, cellHeight * (groupCell.Row + 1) - margin);

                if (groupCell.RightGroup != null)
                    DrawText(groupCell.RightGroup.Sum.ToString(), fontBold, bText,
                        cellWidth * (groupCell.Column + 0.5f) + margin, cellHeight * groupCell.Row + margin,
                        cellWidth * 0.5f - margin * 2f, cellHeight * 0.5f - margin * 2f);

                if (groupCell.DownGroup != null)
                    DrawText(groupCell.DownGroup.Sum.ToString(), fontBold, bText,
                        cellWidth * groupCell.Column + margin, cellHeight * (groupCell.Row + 0.5f) + margin,
                        cellWidth * 0.5f - margin * 2f, cellHeight * 0.5f - margin * 2f);
            }

            if (selectedValueCell != null)
                DrawRect(PuzzlerColor.Black, margin,
                    cellWidth * selectedValueCell.Column + margin, cellHeight * selectedValueCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);
        }

        public override (int Width, int Height) GetPrefferedSize()
        {
            return (40 * GetTrackerBoard().Columns, 40 * GetTrackerBoard().Rows);
        }

        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            BoardKakuru b = this.GetTrackerBoard();
            if (b == null) return;
            int column = (int)(e.X / (sizeX / b.Columns));
            int row    = (int)(e.Y / (sizeY / b.Rows));
            if (row < 0 || row >= b.Rows || column < 0 || column >= b.Columns) return;
            CellValueKakuru pointedCell = b.CellsMatrix[row, column] as CellValueKakuru;
            if (!b.InitialCells.Contains(pointedCell))
                selectedValueCell = pointedCell;
            this.OnRequestRefresh(EventArgs.Empty);
        }

        public override void HandlePointerWheel(PointerEvent e, float sizeX, float sizeY)
        {
            if (selectedValueCell != null)
            {
                if (!selectedValueCell.Value.HasValue)
                {
                    selectedValueCell.Value = 0;
                }
                else
                {
                    int maxValue  = Math.Min(selectedValueCell.Groups[0].Sum, selectedValueCell.Groups[1].Sum);
                    int nextValue = ((int)selectedValueCell.Value + (e.Delta > 0 ? 1 : -1)) % maxValue;
                    selectedValueCell.Value = nextValue < 0 ? nextValue + maxValue : (nextValue == 0 ? maxValue : nextValue);
                }
            }
            this.OnRequestRefresh(EventArgs.Empty);
        }

        public override void HandleKey(KeyEvent e)
        {
            BoardKakuru board = GetTrackerBoard();
            int numRequested = e.KeyValue - 49;
            if (selectedValueCell != null)
                if (numRequested > -1 && numRequested < board.NumberRange.Count)
                    selectedValueCell.Value = board.NumberRange[numRequested];
                else
                    selectedValueCell.Value = null;
            this.OnRequestRefresh(EventArgs.Empty);
        }

        private CellValueKakuru selectedValueCell;
    }
}
