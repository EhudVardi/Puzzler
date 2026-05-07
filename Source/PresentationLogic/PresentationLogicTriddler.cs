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
            DrawAxisArrows(width, height);

            margin = 0;
            foreach (CellValueTriddler valueCell in trackerBoard.ValueCells)
            {
                CellValueTriddler? solvedValueCell = solvedBoard.ValueCells[trackerBoard.ValueCells.IndexOf(valueCell)];
                if (solvedValueCell == null)
                    continue;

                PuzzlerPoint[] cellCoordinates = GetCellTriangle(valueCell.Row, valueCell.Column, valueCell.IsRight, L);

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
                        PuzzlerPoint[] coords = GetCellTriangle(groupCell.Row, groupCell.Column, groupCell.IsRight, L);
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

            // Horizontal axis: triangles extending left from each row's first actual cell.
            // If the first cell is Left(△): slot 0 = Right(r, leftCol-1) sharing its TR-BR with Left's TL-BL.
            // If the first cell is Right(▽): slot 0 = Left(r, leftCol) sharing its diagonal with Right's diagonal.
            for (int r = 0; r < horizGroups.Count; r++)
            {
                List<int> nums = horizGroups[r].Numbers;
                if (nums.Count == 0) continue;
                int leftCol = LeftmostCol(board, r);
                if (leftCol < 0) continue;
                bool firstIsLeft = board.CellsMatrixLeft[r, leftCol] != null;
                for (int k = 0; k < nums.Count; k++)
                {
                    int slot = nums.Count - 1 - k;
                    int col; bool isRight;
                    if (firstIsLeft) { col = leftCol - 1 - slot / 2;       isRight = (slot % 2 == 0); }
                    else             { col = leftCol - (slot + 1) / 2;     isRight = (slot % 2 == 1); }
                    DrawHeaderTriangle(r, col, isRight, nums[k], L, bHoriz);
                }
            }

            // Vertical axis: triangles extending above each column's first actual cell.
            // If the first cell is Right(▽): slot 0 = Left(topRow-1, c) sharing its BL-BR with Right's TL-TR.
            // If the first cell is Left(△): slot 0 = Right(topRow, c) sharing its diagonal with Left's diagonal.
            for (int c = 0; c < vertGroups.Count; c++)
            {
                List<int> nums = vertGroups[c].Numbers;
                if (nums.Count == 0) continue;
                int topRow = TopmostRow(board, c);
                if (topRow < 0) continue;
                bool firstIsRight = board.CellsMatrixRight[topRow, c] != null;
                for (int k = 0; k < nums.Count; k++)
                {
                    int slot = nums.Count - 1 - k;
                    int row; bool isRight;
                    if (firstIsRight) { row = topRow - 1 - slot / 2;       isRight = (slot % 2 == 1); }
                    else              { row = topRow - (slot + 1) / 2;     isRight = (slot % 2 == 0); }
                    DrawHeaderTriangle(row, c, isRight, nums[k], L, bVert);
                }
            }

            // Diagonal axis: alternating triangles stepping down-right from each diagonal's anchor cell.
            foreach (var group in diagGroups)
            {
                List<int> nums = group.Numbers;
                if (nums.Count == 0 || group.Cells.Count == 0) continue;
                var anchor = group.Cells[0] as CellValueTriddler;
                if (anchor == null) continue;
                bool anchorIsRight = anchor.IsRight;
                for (int k = 1; k <= nums.Count; k++)
                {
                    int rOff, cOff; bool stepIsRight;
                    if (anchorIsRight)
                    {
                        rOff       = k / 2;
                        cOff       = (k + 1) / 2;
                        stepIsRight = (k % 2 == 0);
                    }
                    else
                    {
                        rOff       = (k + 1) / 2;
                        cOff       = k / 2;
                        stepIsRight = (k % 2 == 1);
                    }
                    DrawHeaderTriangle(anchor.Row + rOff, anchor.Column + cOff, stepIsRight, nums[nums.Count - k], L, bDiag);
                }
            }
        }

        private static readonly PuzzlerColor bHoriz = PuzzlerColor.FromArgb(255, 160, 190, 240); // cornflower blue
        private static readonly PuzzlerColor bVert  = PuzzlerColor.FromArgb(255, 255, 255, 140); // lemon yellow
        private static readonly PuzzlerColor bDiag  = PuzzlerColor.FromArgb(255, 255, 175, 180); // rose pink

        private void DrawHeaderTriangle(int row, int col, bool isRight, int number, Layout L, PuzzlerColor fill)
        {
            DrawPolygon(PuzzlerColor.Black, 1, fill, GetCellTriangle(row, col, isRight, L));
            float lr = row + L.vertRows;
            float lc = col + L.horizCols;
            float cx, cy;
            if (isRight)
            {
                cx = Vx(lc + 2f / 3f, lr + 1f / 3f, L);
                cy = Vy(lr + 1f / 3f, L);
            }
            else
            {
                cx = Vx(lc + 1f / 3f, lr + 2f / 3f, L);
                cy = Vy(lr + 2f / 3f, L);
            }
            DrawText(number.ToString(), fontBold, bText, cx - L.s * 0.3f, cy - L.h * 0.3f, L.s * 0.6f, L.h * 0.6f);
        }

        // Equilateral triangle cell from grid coordinates (row, col, isRight).
        // Right(▽) = [TL, TR, BR]; Left(△) = [TL, BL, BR].
        private static PuzzlerPoint[] GetCellTriangle(int row, int col, bool isRight, Layout L)
        {
            float lr = row + L.vertRows;
            float lc = col + L.horizCols;
            float tlX = Vx(lc,     lr,     L),  tlY = Vy(lr,     L);
            float trX = Vx(lc + 1, lr,     L),  trY = Vy(lr,     L);
            float blX = Vx(lc,     lr + 1, L),  blY = Vy(lr + 1, L);
            float brX = Vx(lc + 1, lr + 1, L),  brY = Vy(lr + 1, L);
            if (isRight)
                return new[] { new PuzzlerPoint(tlX, tlY), new PuzzlerPoint(trX, trY), new PuzzlerPoint(brX, brY) };
            else
                return new[] { new PuzzlerPoint(tlX, tlY), new PuzzlerPoint(blX, blY), new PuzzlerPoint(brX, brY) };
        }

        // Fixed-size compass rose in the top-left corner, independent of puzzle zoom.
        // All three arrows converge on a common tip point.
        private void DrawAxisArrows(float width, float height)
        {
            float sq32     = (float)Math.Sqrt(3) / 2f;
            float arrowLen = Math.Min(width, height) * 0.07f;
            float headSize = arrowLen * 0.35f;
            float lineW    = Math.Max(1.5f, arrowLen * 0.08f);
            float margin   = arrowLen * 0.5f;

            // Three directions pointing TOWARD the common tip (cx, cy).
            // Horizontal →, Vertical ↙, Diagonal ↖ (inverted — reading direction into board).
            (float dx, float dy, PuzzlerColor col)[] axes =
            {
                ( 1f,    0f,   bHoriz),   // →
                (-0.5f,  sq32, bVert ),   // ↙
                (-0.5f, -sq32, bDiag ),   // ↖
            };

            // Place the tip so every tail stays on-canvas:
            //   → tail is at (cx - arrowLen), needs > margin       → cx > arrowLen + margin
            //   ↙ tail is at (cy - sq32*arrowLen), needs > margin  → cy > sq32*arrowLen + margin
            //   ↖ tail is at (cy + sq32*arrowLen), stays low       → fine for top-left placement
            float cx = arrowLen + margin * 2f;
            float cy = sq32 * arrowLen + margin * 2f;

            foreach (var (dx, dy, col) in axes)
                DrawArrow(cx - dx * arrowLen, cy - dy * arrowLen, dx, dy, arrowLen, headSize, lineW, col);
        }

        private void DrawArrow(float x, float y, float dx, float dy, float len, float headSize, float lineW, PuzzlerColor color)
        {
            float tipX  = x + dx * len,              tipY  = y + dy * len;
            float baseX = tipX - dx * headSize,       baseY = tipY - dy * headSize;
            DrawLine(color, lineW, x, y, baseX, baseY);
            float px = -dy, py = dx;   // perpendicular unit vector
            float hw = headSize * 0.45f;
            DrawPolygon(color, 0, color, new[]
            {
                new PuzzlerPoint(tipX, tipY),
                new PuzzlerPoint(baseX + px * hw, baseY + py * hw),
                new PuzzlerPoint(baseX - px * hw, baseY - py * hw),
            });
        }

        // Screen X of layout point (layoutCol, layoutRow).
        private static float Vx(float layoutCol, float layoutRow, Layout L) =>
            layoutCol * L.s + L.oX - layoutRow * L.s / 2;

        // Screen Y of layout row.
        private static float Vy(float layoutRow, Layout L) =>
            layoutRow * L.h + L.oY;

        private struct Layout
        {
            public float s;         // equilateral triangle edge length
            public float h;         // row height = s·√3/2
            public float oX;        // screen X origin (layout col 0, row 0)
            public float oY;        // screen Y origin
            public int   horizCols; // ⌈maxH/2⌉ — layout columns reserved for horizontal headers
            public int   vertRows;  // ⌈maxV/2⌉ — layout rows reserved for vertical headers
            public int   diagSlots; // ⌈maxD/2⌉ — extra rows/cols for diagonal headers
            public int   maxH;      // raw max horizontal pattern count
            public int   maxV;      // raw max vertical pattern count
            public int   maxD;      // raw max diagonal pattern count
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
            int horizCols = (hMax + 1) / 2;
            int vertRows  = (vMax + 1) / 2;
            int diagSlots = (dMax + 1) / 2;
            int totalCols = horizCols + b.Columns + diagSlots;
            int totalRows = vertRows  + b.Rows    + diagSlots;
            float sqrt3over2 = (float)Math.Sqrt(3) / 2f;
            float sW = width  / (totalCols + totalRows * 0.5f);
            float sH = totalRows > 0 ? height / (totalRows * sqrt3over2) : sW;
            float s  = Math.Min(sW, sH);
            float h  = s * sqrt3over2;
            float oX = (width  - (totalCols + totalRows * 0.5f) * s) / 2f + totalRows * s / 2f;
            float oY = (height - totalRows * h) / 2f;
            return new Layout
            {
                s         = s,
                h         = h,
                oX        = oX,
                oY        = oY,
                horizCols = horizCols,
                vertRows  = vertRows,
                diagSlots = diagSlots,
                maxH      = hMax,
                maxV      = vMax,
                maxD      = dMax,
            };
        }

        public override (int Width, int Height) GetPrefferedSize()
        {
            BoardTriddler? b = GetTrackerBoard();
            if (b == null) return (40 * 10, 40 * 10);
            var (hMax, vMax, dMax) = GetMaxPatternLengths(b);
            int horizCols = (hMax + 1) / 2;
            int vertRows  = (vMax + 1) / 2;
            int diagSlots = (dMax + 1) / 2;
            int totalCols = horizCols + b.Columns + diagSlots;
            int totalRows = vertRows  + b.Rows    + diagSlots;
            int w = (int)((totalCols + totalRows * 0.5) * 40);
            int h = (int)(totalRows * 40 * Math.Sqrt(3) / 2);
            return (Math.Max(w, 100), Math.Max(h, 100));
        }

        // Leftmost surviving column in the given grid row (-1 if entire row is null).
        private static int LeftmostCol(BoardTriddler board, int row)
        {
            for (int c = 0; c < board.Columns; c++)
                if (board.CellsMatrixLeft[row, c] != null || board.CellsMatrixRight[row, c] != null)
                    return c;
            return -1;
        }

        // Topmost surviving row in the given grid column (-1 if entire column is null).
        private static int TopmostRow(BoardTriddler board, int col)
        {
            for (int r = 0; r < board.Rows; r++)
                if (board.CellsMatrixLeft[r, col] != null || board.CellsMatrixRight[r, col] != null)
                    return r;
            return -1;
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
            if (L.s <= 0) return ((-1, -1), false);
            double yIdx = (e.Y - L.oY) / L.h;
            int layoutRow = (int)Math.Floor(yIdx);
            double xIdx = (e.X - L.oX + layoutRow * L.s / 2) / L.s;
            int layoutCol = (int)Math.Floor(xIdx);
            double fracR = yIdx - layoutRow;
            double fracC = xIdx - layoutCol;
            int row = layoutRow - L.vertRows;
            int col = layoutCol - L.horizCols;
            return ((row, col), fracC > fracR);
        }

        private CellValueTriddler? selectedValueCell;
    }
}
