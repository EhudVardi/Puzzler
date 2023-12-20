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

namespace Presentation
{
    public class PresentationLogicKakuru : PresentationLogicGeneric<PuzzleKakuru, BoardKakuru>
    {

        public PresentationLogicKakuru()
        {
            this.LogicProxy = new LogicLayerKakuru();

            this.URL = "http://www.kakuroconquest.com/9x11/expert";
        }



        public override void Draw(PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;

                float brWidth = e.ClipRectangle.Width;
                float brHeight = e.ClipRectangle.Height;

                float cellWidth = (float)brWidth / GetTrackerBoard().Columns;
                float cellHeight = (float)brHeight / GetTrackerBoard().Rows;

                //draw value cells
                foreach (CellValueKakuru valueCell in GetTrackerBoard().ValueCells)
                {
                    CellValueKakuru solvedValueCell = GetSolvedBoard().CellsMatrix[valueCell.Row, valueCell.Column] as CellValueKakuru;

                    Brush brushBackColor;

                    if (!GetTrackerBoard().InitialCells.Contains(valueCell))
                        brushBackColor = bNull;
                    else 
                        brushBackColor = bFixed;

                    //draw value cell back color (initial or not)
                    g.FillRectangle(brushBackColor, 
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
                                g.DrawString(valueCell.Value.ToString(), font, bText,
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

                                g.DrawString(valueCell.Value.ToString(), font, brushValue,
                                    new RectangleF(
                                    cellWidth * valueCell.Column + margin,
                                    cellHeight * valueCell.Row + margin,
                                    cellWidth - margin * 2f,
                                    cellHeight - margin * 2f
                                    ), sf);
                            }
                            break;
                        case DisplayType.Solution:
                            g.DrawString(solvedValueCell.Value.ToString(), font, bCorrect,
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

                        g.DrawString(valueCell.Value.ToString(), font, brushValue,
                            new RectangleF(
                            cellWidth * valueCell.Column + margin,
                            cellHeight * valueCell.Row + margin,
                            cellWidth - margin * 2f,
                            cellHeight - margin * 2f
                            ), sf);
                    }

                }


                //draw group holder cells
                foreach (CellGroupHolderKakuru groupCell in GetTrackerBoard().GroupHolderCells)
                {
                    //draw the rectangle
                    g.FillRectangle(bGroupHolder, 
                        cellWidth * groupCell.Column + margin, 
                        cellHeight * groupCell.Row + margin, 
                        cellWidth - margin * 2f, 
                        cellHeight - margin * 2f
                        );

                    //draw a cross
                    g.DrawLine(new Pen(Color.Black, margin), 
                        cellWidth * groupCell.Column + margin, 
                        cellHeight * groupCell.Row + margin, 
                        cellWidth * (groupCell.Column + 1) - margin, 
                        cellHeight * (groupCell.Row + 1) - margin
                        );

                    //draw right line sum
                    if (groupCell.RightGroup != null)
                        g.DrawString(groupCell.RightGroup.Sum.ToString(), font, bText, 
                            new RectangleF(
                            cellWidth * ((float)groupCell.Column + 0.5f) + margin, 
                            cellHeight * groupCell.Row + margin, 
                            cellWidth * 0.5f - margin * 2f, 
                            cellHeight * 0.5f - margin * 2f
                            ), sf);

                    //draw down line sum
                    if (groupCell.DownGroup != null)
                        g.DrawString(groupCell.DownGroup.Sum.ToString(), font, bText, 
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
                    g.DrawRectangle(new Pen(Color.Black, margin),
                        cellWidth * selectedValueCell.Column + margin, 
                        cellHeight * selectedValueCell.Row + margin, 
                        cellWidth - margin * 2f, 
                        cellHeight - margin * 2f
                        );
                }

            }
            catch (Exception)
            {
                base.Draw(e);
            }
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
            catch (Exception ex)
            {

            }
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
