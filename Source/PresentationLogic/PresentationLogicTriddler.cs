using System;
using System.Collections.Generic;
using System.Linq;
using Logic;
using Data.DataModels;
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
            var L = ComputeLayout(trackerBoard, width, height);

            DrawHeaders(trackerBoard, L);

            margin = 0;
            foreach (CellValueTriddler valueCell in trackerBoard.ValueCells)
            {
                CellValueTriddler? solvedValueCell = solvedBoard.ValueCells[trackerBoard.ValueCells.IndexOf(valueCell)];
                if (solvedValueCell == null)
                    continue;

                PuzzlerPoint[] cellCoordinates = GetTriangleCoordinates(L.cellSize, valueCell.Row, valueCell.Column, valueCell.IsRight, L.gridOriginX, L.gridOriginY);

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
                        if (solvedValueCell.IsFixed)
                        {
                            PuzzlerColor fill2 = solvedValueCell.Value.HasValue
                                ? (solvedValueCell.Value.Value ? PuzzlerColor.Green : PuzzlerColor.Red)
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
                        PuzzlerPoint[] coords = GetTriangleCoordinates(L.cellSize, groupCell.Row, groupCell.Column, groupCell.IsRight, L.gridOriginX, L.gridOriginY);
                        DrawPolygon(PuzzlerColor.Black, 1, PuzzlerColor.Wheat.WithAlpha(128), coords);
                    }
                }
            }
        }

        private void DrawHeaders(BoardTriddler board, Layout L)
        {
            var horizGroups = board.Groups.OfType<GroupTriddlerHorizontal>().ToList();
            var vertGroups  = board.Groups.OfType<GroupTriddlerVerical>().ToList();
            var diagGroups  = board.Groups.OfType<GroupTriddlerDiagonal>().ToList();

            for (int r = 0; r < horizGroups.Count; r++)
            {
                List<int> nums = horizGroups[r].Numbers;
                int offset = L.maxHorizPatternLen - nums.Count;
                float cy = L.gridOriginY + L.cellSize * r;
                for (int k = 0; k < nums.Count; k++)
                {
                    float cx = L.originX + L.cellSize * (offset + k);
                    DrawHeaderRect(cx, cy, L.cellSize, nums[k]);
                }
            }

            for (int c = 0; c < vertGroups.Count; c++)
            {
                List<int> nums = vertGroups[c].Numbers;
                int offset = L.maxVertPatternLen - nums.Count;
                float cx = L.gridOriginX + L.cellSize * c;
                for (int k = 0; k < nums.Count; k++)
                {
                    float cy = L.originY + L.cellSize * (offset + k);
                    DrawHeaderRect(cx, cy, L.cellSize, nums[k]);
                }
            }

            int R = board.Rows;
            int C = board.Columns;
            for (int d = 0; d < diagGroups.Count; d++)
            {
                List<int> nums = diagGroups[d].Numbers;
                int anchorRow, anchorCol;
                bool anchorIsRight;
                if (d < R)
                { anchorRow = d;     anchorCol = C - 1;          anchorIsRight = true;  }
                else
                { anchorRow = R - 1; anchorCol = R + C - 1 - d;  anchorIsRight = false; }

                for (int k = 1; k <= nums.Count; k++)
                {
                    int rOff, cOff;
                    bool stepIsRight;
                    if (anchorIsRight)
                    {
                        rOff = k / 2;
                        cOff = (k + 1) / 2;
                        stepIsRight = (k % 2 == 0);
                    }
                    else
                    {
                        rOff = (k + 1) / 2;
                        cOff = k / 2;
                        stepIsRight = (k % 2 == 1);
                    }
                    int triRow = anchorRow + rOff;
                    int triCol = anchorCol + cOff;

                    PuzzlerPoint[] coords = GetTriangleCoordinates(L.cellSize, triRow, triCol, stepIsRight, L.gridOriginX, L.gridOriginY);
                    DrawPolygon(PuzzlerColor.Black, 1, bFixed, coords);

                    float tx, ty;
                    if (stepIsRight)
                    {
                        tx = L.gridOriginX + L.cellSize * triCol + L.cellSize * 5f / 12f;
                        ty = L.gridOriginY + L.cellSize * triRow + L.cellSize * 1f / 12f;
                    }
                    else
                    {
                        tx = L.gridOriginX + L.cellSize * triCol + L.cellSize * 1f / 12f;
                        ty = L.gridOriginY + L.cellSize * triRow + L.cellSize * 5f / 12f;
                    }
                    DrawText(nums[k - 1].ToString(), fontBold, bText, tx, ty, L.cellSize / 2f, L.cellSize / 2f);
                }
            }
        }

        private void DrawHeaderRect(float x, float y, float size, int number)
        {
            const float m = 2f;
            FillRect(bFixed, x + m, y + m, size - m * 2f, size - m * 2f);
            DrawText(number.ToString(), fontBold, bText, x + m, y + m, size - m * 2f, size - m * 2f);
        }

        private static PuzzlerPoint[] GetTriangleCoordinates(float cellSize, int row, int col, bool isRight, float originX, float originY)
        {
            float x0 = originX + cellSize * col;
            float y0 = originY + cellSize * row;
            if (isRight)
            {
                return new[]
                {
                    new PuzzlerPoint(x0,            y0),
                    new PuzzlerPoint(x0 + cellSize, y0),
                    new PuzzlerPoint(x0 + cellSize, y0 + cellSize),
                };
            }
            return new[]
            {
                new PuzzlerPoint(x0,            y0),
                new PuzzlerPoint(x0,            y0 + cellSize),
                new PuzzlerPoint(x0 + cellSize, y0 + cellSize),
            };
        }

        private struct Layout
        {
            public float originX;
            public float originY;
            public float gridOriginX;
            public float gridOriginY;
            public float cellSize;
            public int maxHorizPatternLen;
            public int maxVertPatternLen;
            public int maxDiagPatternLen;
        }

        private static (int hMax, int vMax, int dMax) GetMaxPatternLengths(BoardTriddler b)
        {
            int hMax = 1, vMax = 1, dMax = 1;
            foreach (var g in b.Groups.OfType<GroupTriddlerHorizontal>())
                if (g.Numbers.Count > hMax) hMax = g.Numbers.Count;
            foreach (var g in b.Groups.OfType<GroupTriddlerVerical>())
                if (g.Numbers.Count > vMax) vMax = g.Numbers.Count;
            foreach (var g in b.Groups.OfType<GroupTriddlerDiagonal>())
                if (g.Numbers.Count > dMax) dMax = g.Numbers.Count;
            return (hMax, vMax, dMax);
        }

        private Layout ComputeLayout(BoardTriddler b, float width, float height)
        {
            var (hMax, vMax, dMax) = GetMaxPatternLengths(b);
            int diagExtra = (dMax + 1) / 2;
            int totalCols = hMax + b.Columns + diagExtra;
            int totalRows = vMax + b.Rows + diagExtra;
            float cellSize = Math.Min(width / totalCols, height / totalRows);
            float originX = (width  - cellSize * totalCols) / 2f;
            float originY = (height - cellSize * totalRows) / 2f;
            return new Layout
            {
                originX = originX,
                originY = originY,
                gridOriginX = originX + cellSize * hMax,
                gridOriginY = originY + cellSize * vMax,
                cellSize = cellSize,
                maxHorizPatternLen = hMax,
                maxVertPatternLen = vMax,
                maxDiagPatternLen = dMax,
            };
        }

        public override (int Width, int Height) GetPrefferedSize()
        {
            BoardTriddler? b = GetTrackerBoard();
            if (b == null) return (40 * 10, 40 * 10);
            var (hMax, vMax, dMax) = GetMaxPatternLengths(b);
            int diagExtra = (dMax + 1) / 2;
            return (40 * (hMax + b.Columns + diagExtra), 40 * (vMax + b.Rows + diagExtra));
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
            if (row > -1 && row < b.Rows && col > -1 && col < b.Columns)
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
            var L = ComputeLayout(b, sizeX, sizeY);
            if (L.cellSize <= 0) return ((-1, -1), false);
            double floatColIndex = (e.X - L.gridOriginX) / L.cellSize;
            double floatRowIndex = (e.Y - L.gridOriginY) / L.cellSize;
            int row = (int)Math.Floor(floatRowIndex);
            int col = (int)Math.Floor(floatColIndex);
            double fracR = floatRowIndex - row;
            double fracC = floatColIndex - col;
            return ((row, col), fracC > fracR);
        }

        private CellValueTriddler? selectedValueCell;
    }
}
