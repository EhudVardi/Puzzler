using System;
using System.Collections.Generic;
using System.Linq;
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

            int hintCols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(trackerBoard.NumberRange.Count)));
            int hintRows = hintCols;
            PuzzlerColor fadedColor = bText.WithAlpha(96);

            foreach (CellValueKakuru valueCell in trackerBoard.ValueCells)
            {
                CellValueKakuru? solvedValueCell = solvedBoard.CellsMatrix[valueCell.Row, valueCell.Column] as CellValueKakuru;
                bool isFixed = trackerBoard.InitialCells.Contains(valueCell);

                PuzzlerColor backColor = isFixed ? bFixed : bNull;
                FillRect(backColor,
                    cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);

                float cx = cellWidth  * valueCell.Column + margin;
                float cy = cellHeight * valueCell.Row    + margin;
                float cw = cellWidth  - margin * 2f;
                float ch = cellHeight - margin * 2f;

                if (this.displayType == DisplayType.Solution)
                {
                    DrawText(solvedValueCell?.Value.ToString() ?? "", font, bCorrect, cx, cy, cw, ch);
                    continue;
                }

                if (valueCell.Value.HasValue)
                {
                    PuzzlerColor textColor;
                    if (this.displayType == DisplayType.Hint)
                        textColor = isFixed
                            ? bText
                            : (solvedValueCell?.Value == valueCell.Value ? bCorrect : bIncorrect);
                    else
                        textColor = isFixed ? bText : bCorrect;

                    DrawText(valueCell.Value.ToString() ?? "", font, textColor, cx, cy, cw, ch);
                }
                else
                {
                    if (ReferenceEquals(valueCell, selectedValueCell) && _activeCandidate.HasValue)
                        DrawText(_activeCandidate.Value.ToString(), fontBold, fadedColor, cx, cy, cw, ch);

                    if (valueCell.Hints.Count > 0)
                    {
                        float subW = cw / hintCols;
                        float subH = ch / hintRows;
                        foreach (int hv in valueCell.Hints)
                        {
                            int idx = trackerBoard.NumberRange.IndexOf(hv);
                            if (idx < 0) continue;
                            int sr = idx / hintCols;
                            int sc = idx % hintCols;
                            float sx = cx + sc * subW;
                            float sy = cy + sr * subH;
                            PuzzlerColor hintColor = (_activeCandidate.HasValue && hv == _activeCandidate.Value)
                                ? bCorrect
                                : PuzzlerColor.Gray;
                            DrawText(hv.ToString(), font, hintColor, sx, sy, subW, subH);
                        }
                    }
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
            return (40 * (GetTrackerBoard()?.Columns ?? 10), 40 * (GetTrackerBoard()?.Rows ?? 10));
        }

        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            BoardKakuru? b = this.GetTrackerBoard();
            if (b == null) return;
            int column = (int)(e.X / (sizeX / b.Columns));
            int row    = (int)(e.Y / (sizeY / b.Rows));
            if (row < 0 || row >= b.Rows || column < 0 || column >= b.Columns) return;

            if (b.CellsMatrix[row, column] is not CellValueKakuru pointedCell) return;
            if (b.InitialCells.Contains(pointedCell)) return;

            if (!ReferenceEquals(pointedCell, selectedValueCell))
            {
                selectedValueCell = pointedCell;
                UpdateActiveCandidateForSelection();
            }
            else if (e.Button == PointerButton.Left)
            {
                if (pointedCell.Value.HasValue)
                {
                    pointedCell.Value = null;
                    UpdateActiveCandidateForSelection();
                }
                else if (_activeCandidate.HasValue)
                {
                    pointedCell.Value = _activeCandidate;
                    _activeCandidate = null;
                }
            }
            else if (e.Button == PointerButton.Right)
            {
                if (!pointedCell.Value.HasValue && _activeCandidate.HasValue)
                {
                    if (!pointedCell.Hints.Add(_activeCandidate.Value))
                        pointedCell.Hints.Remove(_activeCandidate.Value);
                }
            }

            this.OnRequestRefresh(EventArgs.Empty);
        }

        public override void HandlePointerWheel(PointerEvent e, float sizeX, float sizeY)
        {
            BoardKakuru? b = GetTrackerBoard();
            if (b == null || selectedValueCell == null) return;
            if (selectedValueCell.Value.HasValue) return;

            List<int> possibles = ComputePossibleValues(selectedValueCell, b);
            if (possibles.Count == 0)
            {
                _activeCandidate = null;
                this.OnRequestRefresh(EventArgs.Empty);
                return;
            }

            int idx = _activeCandidate.HasValue ? possibles.IndexOf(_activeCandidate.Value) : -1;
            int delta = e.Delta > 0 ? 1 : -1;
            int next  = ((idx < 0 ? 0 : idx) + delta) % possibles.Count;
            if (next < 0) next += possibles.Count;
            _activeCandidate = possibles[next];

            this.OnRequestRefresh(EventArgs.Empty);
        }

        private void UpdateActiveCandidateForSelection()
        {
            BoardKakuru? b = GetTrackerBoard();
            if (b == null || selectedValueCell == null || selectedValueCell.Value.HasValue)
            {
                _activeCandidate = null;
                return;
            }
            List<int> possibles = ComputePossibleValues(selectedValueCell, b);
            _activeCandidate = possibles.Count > 0 ? possibles[0] : (int?)null;
        }

        private static List<int> ComputePossibleValues(CellValueKakuru cell, BoardKakuru board)
        {
            HashSet<int> topo = board.GetCellTopologyValues(cell);
            HashSet<int> excluded = new();
            foreach (var group in cell.Groups)
                foreach (var sibling in group.Cells)
                    if (!ReferenceEquals(sibling, cell) && sibling.Value.HasValue)
                        excluded.Add(sibling.Value!.Value);
            return topo.Where(v => !excluded.Contains(v)).OrderBy(v => v).ToList();
        }

        private CellValueKakuru? selectedValueCell;
        private int? _activeCandidate;
    }
}
