using System;
using System.Collections.Generic;
using System.Linq;
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
            var L = ComputeLayout(trackerBoard, width, height);

            DrawHeaders(trackerBoard, L);

            foreach (CellValueGriddler valueCell in trackerBoard.ValueCells)
            {
                CellValueGriddler? solvedValueCell = solvedBoard.CellsMatrix[valueCell.Row, valueCell.Column] as CellValueGriddler;

                float cx = L.gridOriginX + L.cellSize * valueCell.Column;
                float cy = L.gridOriginY + L.cellSize * valueCell.Row;

                switch (this.displayType)
                {
                    case DisplayType.Board:
                        FillRect(
                            valueCell.Value == null ? PuzzlerColor.Yellow
                                : valueCell.Value == true ? PuzzlerColor.Green : PuzzlerColor.Red,
                            cx + margin, cy + margin,
                            L.cellSize - margin * 2f, L.cellSize - margin * 2f);
                        break;

                    case DisplayType.Hint:
                        FillRect(
                            valueCell.Value == null ? PuzzlerColor.Yellow
                                : valueCell.Value == true ? PuzzlerColor.Green : PuzzlerColor.Red,
                            cx + margin, cy + margin,
                            L.cellSize - margin * 2f, L.cellSize - margin * 2f);

                        PuzzlerColor hintStroke = solvedValueCell!.Value != valueCell.Value
                            ? PuzzlerColor.Red : PuzzlerColor.Green;

                        DrawRect(hintStroke, 1,
                            cx + margin, cy + margin,
                            L.cellSize - margin * 2f, L.cellSize - margin * 2f);
                        break;

                    case DisplayType.Solution:
                        if (solvedValueCell!.IsFixed)
                            FillRect(
                                solvedValueCell.Value == null ? PuzzlerColor.Yellow
                                    : solvedValueCell.Value == true ? PuzzlerColor.Blue : PuzzlerColor.Red,
                                cx + margin, cy + margin,
                                L.cellSize - margin * 2f, L.cellSize - margin * 2f);
                        break;
                }
            }

            if (selectedValueCell != null)
                DrawRect(PuzzlerColor.Black, margin,
                    L.gridOriginX + L.cellSize * selectedValueCell.Column + margin,
                    L.gridOriginY + L.cellSize * selectedValueCell.Row + margin,
                    L.cellSize - margin * 2f, L.cellSize - margin * 2f);
        }

        private void DrawHeaders(BoardGriddler board, Layout L)
        {
            var rowGroups = board.Groups.OfType<GroupGriddlerRow>().ToList();
            var colGroups = board.Groups.OfType<GroupGriddlerColumn>().ToList();

            for (int r = 0; r < rowGroups.Count; r++)
            {
                List<int> nums = rowGroups[r].Numbers;
                int offset = L.maxRowPatternLen - nums.Count;
                float cy = L.gridOriginY + L.cellSize * r;
                for (int k = 0; k < nums.Count; k++)
                {
                    float cx = L.originX + L.cellSize * (offset + k);
                    FillRect(bFixed,
                        cx + margin, cy + margin,
                        L.cellSize - margin * 2f, L.cellSize - margin * 2f);
                    DrawText(nums[k].ToString(), fontBold, bText,
                        cx + margin, cy + margin,
                        L.cellSize - margin * 2f, L.cellSize - margin * 2f);
                }
            }

            for (int c = 0; c < colGroups.Count; c++)
            {
                List<int> nums = colGroups[c].Numbers;
                int offset = L.maxColPatternLen - nums.Count;
                float cx = L.gridOriginX + L.cellSize * c;
                for (int k = 0; k < nums.Count; k++)
                {
                    float cy = L.originY + L.cellSize * (offset + k);
                    FillRect(bFixed,
                        cx + margin, cy + margin,
                        L.cellSize - margin * 2f, L.cellSize - margin * 2f);
                    DrawText(nums[k].ToString(), fontBold, bText,
                        cx + margin, cy + margin,
                        L.cellSize - margin * 2f, L.cellSize - margin * 2f);
                }
            }
        }

        private struct Layout
        {
            public float originX;
            public float originY;
            public float gridOriginX;
            public float gridOriginY;
            public float cellSize;
            public int maxRowPatternLen;
            public int maxColPatternLen;
        }

        private static (int rowMax, int colMax) GetMaxPatternLengths(BoardGriddler b)
        {
            int rowMax = 1, colMax = 1;
            foreach (var g in b.Groups.OfType<GroupGriddlerRow>())
                if (g.Numbers.Count > rowMax) rowMax = g.Numbers.Count;
            foreach (var g in b.Groups.OfType<GroupGriddlerColumn>())
                if (g.Numbers.Count > colMax) colMax = g.Numbers.Count;
            return (rowMax, colMax);
        }

        private Layout ComputeLayout(BoardGriddler b, float width, float height)
        {
            var (maxRowPatternLen, maxColPatternLen) = GetMaxPatternLengths(b);
            int totalCols = maxRowPatternLen + b.Columns;
            int totalRows = maxColPatternLen + b.Rows;
            float cellSize = Math.Min(width / totalCols, height / totalRows);
            float originX = (width  - cellSize * totalCols) / 2f;
            float originY = (height - cellSize * totalRows) / 2f;
            return new Layout
            {
                originX = originX,
                originY = originY,
                gridOriginX = originX + cellSize * maxRowPatternLen,
                gridOriginY = originY + cellSize * maxColPatternLen,
                cellSize = cellSize,
                maxRowPatternLen = maxRowPatternLen,
                maxColPatternLen = maxColPatternLen,
            };
        }

        public override (int Width, int Height) GetPrefferedSize()
        {
            BoardGriddler? b = GetTrackerBoard();
            if (b == null) return (40 * 10, 40 * 10);
            var (rowMax, colMax) = GetMaxPatternLengths(b);
            return (40 * (rowMax + b.Columns), 40 * (colMax + b.Rows));
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
            var L = ComputeLayout(b, sizeX, sizeY);
            if (L.cellSize <= 0) return (-1, -1);
            return ((int)Math.Floor((e.Y - L.gridOriginY) / L.cellSize),
                    (int)Math.Floor((e.X - L.gridOriginX) / L.cellSize));
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
