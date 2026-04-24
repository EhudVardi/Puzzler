using System;
using Logic;
using Data.DataModels;
using Common.Models.Griddler;
using Common.Models.Triddler;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicTriddler : PresentationLogicGeneric<PuzzleTriddler, BoardTriddler>
    {
        public PresentationLogicTriddler()
            : base()
        {
            this.LogicProxy = new LogicLayerTriddler();
        }

        public override void DrawBoard(BoardTriddler trackerBoard, BoardTriddler solvedBoard, float width, float height)
        {
            float cellWidth  = width  / trackerBoard.Columns;
            float cellHeight = height / trackerBoard.Rows;

            foreach (CellValueTriddler valueCell in trackerBoard.ValueCells)
            {
                CellValueTriddler? solvedValueCellTriddler = solvedBoard.ValueCells[trackerBoard.ValueCells.IndexOf(valueCell)];
                if (solvedValueCellTriddler == null)
                    continue;

                margin = 0;
                PuzzlerPoint[] cellCoordinates = GetTriddlerCellTriangleCoordinates(cellWidth, cellHeight, solvedValueCellTriddler);

                switch (this.displayType)
                {
                    case DisplayType.Hint:
                    case DisplayType.Board:
                        PuzzlerColor fill = valueCell.Value.HasValue
                            ? (valueCell.Value.Value ? PuzzlerColor.Green : PuzzlerColor.Red)
                            : PuzzlerColor.Transparent;
                        DrawPolygon(PuzzlerColor.Black, 1, fill, cellCoordinates);
                        break;
                    case DisplayType.Solution:
                        if (solvedValueCellTriddler.IsFixed)
                        {
                            PuzzlerColor fill2 = solvedValueCellTriddler.Value.HasValue
                                ? (solvedValueCellTriddler.Value.Value ? PuzzlerColor.Green : PuzzlerColor.Red)
                                : PuzzlerColor.Yellow;
                            DrawPolygon(PuzzlerColor.Black, 1, fill2, cellCoordinates);
                        }
                        break;
                }
            }

            if (selectedValueCell != null)
            {
                for (int i = 0; i < selectedValueCell.Groups.Count; i++)
                {
                    for (int j = 0; j < selectedValueCell.Groups[i].Cells.Count; j++)
                    {
                        CellValueTriddler? groupCell = selectedValueCell.Groups[i].Cells[j] as CellValueTriddler;
                        if (groupCell == null) continue;
                        PuzzlerPoint[] coords = GetTriddlerCellTriangleCoordinates(cellWidth, cellHeight, groupCell);
                        DrawPolygon(PuzzlerColor.Black, 1, PuzzlerColor.Wheat.WithAlpha(128), coords);
                    }
                }
            }
        }

        private static PuzzlerPoint[] GetTriddlerCellTriangleCoordinates(float cellWidth, float cellHeight, CellValueTriddler cell)
        {
            if (cell.IsRight)
            {
                return new[]
                {
                    new PuzzlerPoint(cellWidth * cell.Column + margin,           cellHeight * cell.Row + margin),
                    new PuzzlerPoint(cellWidth * (cell.Column + 1) - margin,     cellHeight * cell.Row + margin),
                    new PuzzlerPoint(cellWidth * (cell.Column + 1) - margin,     cellHeight * (cell.Row + 1) - margin)
                };
            }
            else
            {
                return new[]
                {
                    new PuzzlerPoint(cellWidth * cell.Column + margin,           cellHeight * cell.Row + margin),
                    new PuzzlerPoint(cellWidth * cell.Column + margin,           cellHeight * (cell.Row + 1) - margin),
                    new PuzzlerPoint(cellWidth * (cell.Column + 1) - margin,     cellHeight * (cell.Row + 1) - margin)
                };
            }
        }

        public override void HandlePointerWheel(PointerEvent e, float sizeX, float sizeY)
        {
            if (selectedValueCell != null)
            {
                if (e.Delta > 0)
                {
                    if (selectedValueCell.Value == null)       selectedValueCell.Value = false;
                    else if (selectedValueCell.Value == false) selectedValueCell.Value = true;
                    else                                       selectedValueCell.Value = null;
                }
                else
                {
                    if (selectedValueCell.Value == null)      selectedValueCell.Value = true;
                    else if (selectedValueCell.Value == true) selectedValueCell.Value = false;
                    else                                      selectedValueCell.Value = null;
                }
            }
            this.OnRequestRefresh(EventArgs.Empty);
        }

        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            BoardTriddler? b = this.GetTrackerBoard();
            if (b == null) return;
            var ((row, col), isRight) = GetBoardCoordinates(e, sizeX, sizeY, b);
            if (row > -1 && row < b.Rows && col > -1 && col < b.Rows)
            {
                CellValueTriddler? pointedCell = (isRight ? b.CellsMatrixRight : b.CellsMatrixLeft)[row, col];
                if (pointedCell != null && !b.InitialCells.Contains(pointedCell))
                    selectedValueCell = pointedCell;
            }
            else
            {
                selectedValueCell = null;
            }
            this.OnRequestRefresh(EventArgs.Empty);
        }

        protected ((int row, int col) pos, bool isRight) GetBoardCoordinates(PointerEvent e, float sizeX, float sizeY, BoardTriddler b)
        {
            float floatColIndex = e.X / (sizeX / b.Columns);
            float floatRowIndex = e.Y / (sizeY / b.Rows);
            return (((int)floatRowIndex, (int)floatColIndex),
                (floatColIndex - (int)floatColIndex) > (floatRowIndex - (int)floatRowIndex));
        }

        private CellValueTriddler? selectedValueCell;
    }
}
