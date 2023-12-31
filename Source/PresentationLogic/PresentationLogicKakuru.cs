using System;
using System.Collections.Generic;
using System.Text;
using Logic.Sudoku;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Logic.Kakuru;
using Data.DataModels;
using Data;
using Logic;
using Common.Models.Base;
using Common.Models.Kakuru;
using Common.Models.Griddler;

namespace PresentationLogic
{
    public class PresentationLogicKakuru : PresentationLogicGeneric<PuzzleKakuru, BoardKakuru>
    {
        public PresentationLogicKakuru()
        {
            this.LogicProxy = new LogicLayerKakuru();
            this.URL = "http://www.kakuroconquest.com/9x11/expert";
        }

        public override void DrawBoard(BoardKakuru trackerBoard, BoardKakuru solvedBoard, object drawingContext, float width, float height)
        {
            float brWidth = width;
            float brHeight = height;

            float cellWidth = (float)brWidth / trackerBoard.Columns;
            float cellHeight = (float)brHeight / trackerBoard.Rows;

            //draw value cells
            foreach (CellValueKakuru valueCell in trackerBoard.ValueCells)
            {
                CellValueKakuru solvedValueCell = solvedBoard.CellsMatrix[valueCell.Row, valueCell.Column] as CellValueKakuru;

                Brush brushBackColor;

                if (!trackerBoard.InitialCells.Contains(valueCell))
                    brushBackColor = bNull;
                else
                    brushBackColor = bFixed;

                //draw value cell back color (initial or not)
                OnRequestFillRectangle(drawingContext,
                    brushBackColor,
                    cellWidth * valueCell.Column + margin,
                    cellHeight * valueCell.Row + margin,
                    cellWidth - margin * 2f,
                    cellHeight - margin * 2f
                    );

                //draw value cell value
                switch (this.displayType)
                {
                    case DisplayType.Board:
                        if (valueCell.IsFixed)
                        {
                            OnRequestDrawText(drawingContext,
                                valueCell.Value.ToString(), font, bText,
                                new RectangleF(
                                cellWidth * valueCell.Column + margin,
                                cellHeight * valueCell.Row + margin,
                                cellWidth - margin * 2f,
                                cellHeight - margin * 2f
                                ), sf);
                        }
                        break;
                    case DisplayType.Hint:
                        if (valueCell.IsFixed)
                        {
                            Brush brushValue;
                            if (solvedValueCell.Value != valueCell.Value)
                                brushValue = bIncorrect;
                            else
                                brushValue = bCorrect;

                            OnRequestDrawText(drawingContext,
                                valueCell.Value.ToString(), font, brushValue,
                                new RectangleF(
                                cellWidth * valueCell.Column + margin,
                                cellHeight * valueCell.Row + margin,
                                cellWidth - margin * 2f,
                                cellHeight - margin * 2f
                                ), sf);
                        }
                        break;
                    case DisplayType.Solution:
                        OnRequestDrawText(drawingContext,
                        solvedValueCell.Value.ToString(), font, bCorrect,
                            new RectangleF(
                            cellWidth * valueCell.Column + margin,
                            cellHeight * valueCell.Row + margin,
                            cellWidth - margin * 2f,
                            cellHeight - margin * 2f
                            ), sf);
                        break;
                    default:
                        break;
                }
                if (valueCell.IsFixed)
                {
                    Brush brushValue;
                    if (solvedValueCell.Value != valueCell.Value)
                        brushValue = bIncorrect;
                    else
                        brushValue = bCorrect;

                    OnRequestDrawText(drawingContext,
                        valueCell.Value.ToString(), font, brushValue,
                        new RectangleF(
                        cellWidth * valueCell.Column + margin,
                        cellHeight * valueCell.Row + margin,
                        cellWidth - margin * 2f,
                        cellHeight - margin * 2f
                        ), sf);
                }
            }

            //draw group holder cells
            foreach (CellGroupHolderKakuru groupCell in trackerBoard.GroupHolderCells)
            {
                //draw the rectangle
                OnRequestFillRectangle(drawingContext,
                    bGroupHolder,
                    cellWidth * groupCell.Column + margin,
                    cellHeight * groupCell.Row + margin,
                    cellWidth - margin * 2f,
                    cellHeight - margin * 2f
                    );

                //draw a cross
                OnRequestDrawLine(drawingContext,
                    new Pen(Color.Black, margin),
                    cellWidth * groupCell.Column + margin,
                    cellHeight * groupCell.Row + margin,
                    cellWidth * (groupCell.Column + 1) - margin,
                    cellHeight * (groupCell.Row + 1) - margin
                    );

                //draw right line sum string
                if (groupCell.RightGroup != null)
                    OnRequestDrawText(drawingContext,
                        groupCell.RightGroup.Sum.ToString(), fontBold, bText,
                        new RectangleF(
                        cellWidth * ((float)groupCell.Column + 0.5f) + margin,
                        cellHeight * groupCell.Row + margin,
                        cellWidth * 0.5f - margin * 2f,
                        cellHeight * 0.5f - margin * 2f
                        ), sf);

                //draw down line sum string
                if (groupCell.DownGroup != null)
                    OnRequestDrawText(drawingContext,
                        groupCell.DownGroup.Sum.ToString(), fontBold, bText,
                        new RectangleF(
                        cellWidth * groupCell.Column + margin,
                        cellHeight * ((float)groupCell.Row + 0.5f) + margin,
                        cellWidth * 0.5f - margin * 2f,
                        cellHeight * 0.5f - margin * 2f
                        ), sf);
            }

            //draw selected value cell marker
            if (selectedValueCell != null)
            {
                OnRequestDrawRectangle(drawingContext,
                    new Pen(Color.Black, margin),
                    cellWidth * selectedValueCell.Column + margin,
                    cellHeight * selectedValueCell.Row + margin,
                    cellWidth - margin * 2f,
                    cellHeight - margin * 2f
                    );
            }
        }

        public override Size GetPrefferedSize()
        {
            return new System.Drawing.Size(40 * GetTrackerBoard().Columns, 40 * GetTrackerBoard().Rows);
        }
        
        public override void InputMouse(MouseEventArgs e, Size s)
        {
            try
            {
                BoardKakuru b = this.GetTrackerBoard();
                int column = (int)((float)e.X / ((float)s.Width / (float)b.Columns));
                int row = (int)((float)e.Y / ((float)s.Height / (float)b.Rows));
                CellValueKakuru pointedCell = b.CellsMatrix[row, column] as CellValueKakuru;
                if (!b.InitialCells.Contains(pointedCell))
                    selectedValueCell = pointedCell;
                this.OnRequestRefresh(EventArgs.Empty);
            }
            catch (Exception ex) { }
        }

        public override void InputMouseWheel(MouseEventArgs e, Size size)
        {
            // for sudoku. can refactor to generic board to set specific cell with the next possible value (SetCellNextValue)
            if (selectedValueCell != null)
                if (selectedValueCell.Value.HasValue == false)
                    // set to first value
                    selectedValueCell.Value = 0;
                else
                {
                    // set next value (if wheel rolled forward) or backwards (if wheel rolled backwards)
                    //int maxValue = Math.Max(GetTrackerBoard().Rows, GetTrackerBoard().Columns);
                    int maxValue = Math.Min(selectedValueCell.Groups[0].Sum, selectedValueCell.Groups[1].Sum);
                    int nextValue = ((int)(selectedValueCell.Value) + (e.Delta > 0 ? 1 : -1)) % maxValue;
                    nextValue = nextValue < 0 ? nextValue + maxValue : (nextValue == 0 ? maxValue : nextValue);
                    selectedValueCell.Value = nextValue;
                }
            this.OnRequestRefresh(EventArgs.Empty);
        }

        public override void InputKey(KeyEventArgs e)
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

        #region specific

        CellValueKakuru selectedValueCell;

        #endregion
    }
}
