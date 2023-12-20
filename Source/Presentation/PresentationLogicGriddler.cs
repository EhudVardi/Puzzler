using System;
using System.Collections.Generic;
using System.Text;
using Logic.Sudoku;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Data.DataModels;
using Logic.Griddler;
using Data;
using Logic;
using Common.Models.Griddler;

namespace Presentation
{
    public class PresentationLogicGriddler : PresentationLogicGeneric<PuzzleGriddler, BoardGriddler>
    {
        public PresentationLogicGriddler()
            : base()
        {
            this.LogicProxy = new LogicLayerGriddler();
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
                foreach (CellValueGriddler valueCell in GetTrackerBoard().ValueCells)
                {
                    CellValueGriddler solvedValueCell = GetSolvedBoard().CellsMatrix[valueCell.Row, valueCell.Column] as CellValueGriddler;

                    //draw value cell value
                    switch (this.displayType)
                    {
                        case DisplayType.Board:
                            g.FillRectangle(valueCell.Value == null ? Brushes.Yellow : valueCell.Value == true ? Brushes.Green : Brushes.Red,

                                    cellWidth * valueCell.Column + margin,
                                    cellHeight * valueCell.Row + margin,
                                    cellWidth - margin * 2f,
                                    cellHeight - margin * 2f

                                    );
                            
                            break;
                        case DisplayType.Hint:
                            g.FillRectangle(valueCell.Value == null ? Brushes.Yellow : valueCell.Value == true ? Brushes.Green : Brushes.Red,

                                    cellWidth * valueCell.Column + margin,
                                    cellHeight * valueCell.Row + margin,
                                    cellWidth - margin * 2f,
                                    cellHeight - margin * 2f

                                    );
                            Pen brushValue;
                                if (solvedValueCell.Value != valueCell.Value)
                                    brushValue = Pens.Red;
                                else
                                    brushValue = Pens.Green;

                                g.DrawRectangle(brushValue,
                                    cellWidth * valueCell.Column + margin,
                                    cellHeight * valueCell.Row + margin,
                                    cellWidth - margin * 2f,
                                    cellHeight - margin * 2f
                                    );

                            break;
                        case DisplayType.Solution:
                            if (solvedValueCell.IsFixed)
                            {
                                g.FillRectangle(solvedValueCell.Value == null ? Brushes.Yellow : solvedValueCell.Value == true ? Brushes.Green : Brushes.Red,

                                    cellWidth * solvedValueCell.Column + margin,
                                    cellHeight * solvedValueCell.Row + margin,
                                    cellWidth - margin * 2f,
                                    cellHeight - margin * 2f

                                    );
                            }
                            break;
                        default:
                            break;
                    }
                }

                //draw selected value cell marker
                if (selectedValueCell != null)
                    g.DrawRectangle(new Pen(Color.Black, margin),

                        cellWidth * selectedValueCell.Column + margin,
                        cellHeight * selectedValueCell.Row + margin,
                        cellWidth - margin * 2f,
                        cellHeight - margin * 2f

                        );

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
                BoardGriddler b = this.GetTrackerBoard();

                Point p = GetBoardCoordinates(e, s, b);

                CellValueGriddler pointedCell = b.CellsMatrix[p.X, p.Y] as CellValueGriddler;

                if (!b.InitialCells.Contains(pointedCell))
                    selectedValueCell = pointedCell;

                this.OnRequestRefresh(EventArgs.Empty);
            }
            catch (Exception ex)
            {

            }
        }



        protected  Point GetBoardCoordinates(MouseEventArgs e, Size s, BoardGriddler b)
        {
            return new Point((int)((float)e.X / ((float)s.Width / (float)b.Columns)), (int)((float)e.Y / ((float)s.Height / (float)b.Rows)));
        }



        public override void InputKey(KeyEventArgs e)
        {
            BoardGriddler board = GetTrackerBoard();

            int numRequested = e.KeyValue - 49;

            if (selectedValueCell != null)
                if (numRequested > -1 && numRequested < 3)
                    if (numRequested == 0)
                        selectedValueCell.Value = null;
                    else if (numRequested == 1)
                        selectedValueCell.Value = true;
                    else if (numRequested == 2)
                        selectedValueCell.Value = false;
                    else
                        selectedValueCell.Value = null;


            this.OnRequestRefresh(EventArgs.Empty);
        }


        #region specific


        CellValueGriddler selectedValueCell;


        #endregion
    }
}
