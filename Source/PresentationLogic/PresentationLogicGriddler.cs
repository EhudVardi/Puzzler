using System;
using Logic;
using Data.DataModels;
using Common.Models.Griddler;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicGriddler : PresentationLogicGeneric<PuzzleGriddler, BoardGriddler>
    {
        public PresentationLogicGriddler()
            : base()
        {
            this.LogicProxy = new LogicLayerGriddler();
        }

        public override void DrawBoard(BoardGriddler trackerBoard, BoardGriddler solvedBoard, float width, float height)
        {
            float cellWidth  = width  / trackerBoard.Columns;
            float cellHeight = height / trackerBoard.Rows;

            foreach (CellValueGriddler valueCell in trackerBoard.ValueCells)
            {
                CellValueGriddler? solvedValueCell = solvedBoard.CellsMatrix[valueCell.Row, valueCell.Column] as CellValueGriddler;

                switch (this.displayType)
                {
                    case DisplayType.Board:
                        FillRect(
                            valueCell.Value == null ? PuzzlerColor.Yellow
                                : valueCell.Value == true ? PuzzlerColor.Green : PuzzlerColor.Red,
                            cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                            cellWidth - margin * 2f, cellHeight - margin * 2f);
                        break;

                    case DisplayType.Hint:
                        FillRect(
                            valueCell.Value == null ? PuzzlerColor.Yellow
                                : valueCell.Value == true ? PuzzlerColor.Green : PuzzlerColor.Red,
                            cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                            cellWidth - margin * 2f, cellHeight - margin * 2f);

                        PuzzlerColor hintStroke = solvedValueCell!.Value != valueCell.Value
                            ? PuzzlerColor.Red : PuzzlerColor.Green;

                        DrawRect(hintStroke, 1,
                            cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                            cellWidth - margin * 2f, cellHeight - margin * 2f);
                        break;

                    case DisplayType.Solution:
                        if (solvedValueCell!.IsFixed)
                            FillRect(
                                solvedValueCell.Value == null ? PuzzlerColor.Yellow
                                    : solvedValueCell.Value == true ? PuzzlerColor.Blue : PuzzlerColor.Red,
                                cellWidth * solvedValueCell.Column + margin, cellHeight * solvedValueCell.Row + margin,
                                cellWidth - margin * 2f, cellHeight - margin * 2f);
                        break;
                }
            }

            if (selectedValueCell != null)
                DrawRect(PuzzlerColor.Black, margin,
                    cellWidth * selectedValueCell.Column + margin, cellHeight * selectedValueCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);
        }

        public override (int Width, int Height) GetPrefferedSize()
        {
            return (40 * (GetTrackerBoard()?.Columns ?? 10), 40 * (GetTrackerBoard()?.Rows ?? 10));
        }

        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            BoardGriddler? b = this.GetTrackerBoard();
            if (b == null) return;
            var (row, col) = GetBoardCoordinates(e, sizeX, sizeY, b);
            if (row < 0 || row >= b.Rows || col < 0 || col >= b.Columns) return;
            CellValueGriddler? pointedCell = b.CellsMatrix[row, col] as CellValueGriddler;
            if (pointedCell != null && !b.InitialCells.Contains(pointedCell))
                selectedValueCell = pointedCell;
            this.OnRequestRefresh(EventArgs.Empty);
        }

        protected (int row, int col) GetBoardCoordinates(PointerEvent e, float sizeX, float sizeY, BoardGriddler b)
        {
            return ((int)(e.Y / (sizeY / b.Rows)), (int)(e.X / (sizeX / b.Columns)));
        }

        public override void HandleKey(KeyEvent e)
        {
            if (selectedValueCell == null) return;
            int numRequested = e.KeyValue - 49;
            if (numRequested > -1 && numRequested < 3)
                selectedValueCell.Value = numRequested == 0 ? null : numRequested == 1 ? true : (bool?)false;
            else
                selectedValueCell.Value = null;
            this.OnRequestRefresh(EventArgs.Empty);
        }

        protected CellValueGriddler? selectedValueCell;
    }
}
