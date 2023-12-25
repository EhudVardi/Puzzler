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
using Common.Models.Triddler;
using Common.Models.Kakuru;

namespace PresentationLogic
{

    public class PresentationLogicTriddler : PresentationLogicGeneric<PuzzleTriddler, BoardTriddler>
    {
        public PresentationLogicTriddler()
            : base()
        {
            this.LogicProxy = new LogicLayerTriddler();

        }

        public override void DrawBoard(BoardTriddler trackerBoard, BoardTriddler solvedBoard, object drawingContext, float width, float height)
        {
            float brWidth = width;
            float brHeight = height;

            float cellWidth = (float)brWidth / trackerBoard.Columns;
            float cellHeight = (float)brHeight / trackerBoard.Rows;

            //draw value cells
            foreach (Common.Models.Griddler.CellValueGriddler valueCell in trackerBoard.ValueCells)
            {
                Common.Models.Griddler.CellValueGriddler solvedValueCell = solvedBoard.ValueCells[trackerBoard.ValueCells.IndexOf(valueCell as CellValueTriddler)] as Common.Models.Griddler.CellValueGriddler;

                CellValueTriddler solvedValueCellTriddler = (solvedValueCell as CellValueTriddler);
                if (solvedValueCellTriddler == null)
                    continue;

                margin = 0;
                PointF[] cellCoordinates = GetTriddlerCellTriangleCoordinates(cellWidth, cellHeight, solvedValueCellTriddler);


                //draw value cell value
                switch (this.displayType)
                {
                    case DisplayType.Hint:
                    case DisplayType.Board:
                        Pen pen = Pens.Black;
                        Brush brush = valueCell.Value.HasValue == true ? (valueCell.Value.Value == true ? Brushes.Green : Brushes.Red) : Brushes.Transparent;
                        OnRequestDrawPolygon(drawingContext, pen, brush as SolidBrush, cellCoordinates);
                        break;
                    case DisplayType.Solution:
                        if (solvedValueCellTriddler.IsFixed)
                        {
                            Pen pen2 = Pens.Black;
                            Brush brush2 = solvedValueCellTriddler.Value.HasValue == true ? (solvedValueCellTriddler.Value.Value == true ? Brushes.Green : Brushes.Red) : Brushes.Yellow;
                            OnRequestDrawPolygon(drawingContext, pen2, brush2 as SolidBrush, cellCoordinates);
                        }
                        break;
                    default:
                        break;
                }



            }

            ////draw selected value cell marker
            if (selectedValueCell != null)
            {
                for (int i = 0; i < selectedValueCell.Groups.Count; i++)
                {
                    for (int j = 0; j < selectedValueCell.Groups[i].Cells.Count; j++)
                    {

                        PointF[] cellCoordinates = GetTriddlerCellTriangleCoordinates(cellWidth, cellHeight, selectedValueCell.Groups[i].Cells[j] as CellValueTriddler);
                        Brush brush3 = new SolidBrush(Color.FromArgb(128, Color.Wheat));
                        OnRequestDrawPolygon(drawingContext, Pens.Black, brush3 as SolidBrush, cellCoordinates);
                    }
                }
            }
        }

        private static PointF[] GetTriddlerCellTriangleCoordinates(float cellWidth, float cellHeight, CellValueTriddler solvedValueCellTriddler)
        {
            PointF[] cellCoordinates = null;
            if (solvedValueCellTriddler.IsRight)
            {
                cellCoordinates = new PointF[] 
                                    {
                                         new PointF(cellWidth * solvedValueCellTriddler.Column + margin, cellHeight * solvedValueCellTriddler.Row + margin),
                                         new PointF(cellWidth * (solvedValueCellTriddler.Column + 1) - margin, cellHeight * solvedValueCellTriddler.Row + margin),
                                         new PointF(cellWidth * (solvedValueCellTriddler.Column + 1) - margin, cellHeight * (solvedValueCellTriddler.Row + 1) - margin)
                                    };
            }
            else
            {
                cellCoordinates = new PointF[] 
                                    {
                                         new PointF(cellWidth * solvedValueCellTriddler.Column + margin,cellHeight * solvedValueCellTriddler.Row + margin),
                                         new PointF(cellWidth * solvedValueCellTriddler.Column + margin,cellHeight * (solvedValueCellTriddler.Row + 1) - margin),
                                         new PointF(cellWidth * (solvedValueCellTriddler.Column + 1) - margin,cellHeight * (solvedValueCellTriddler.Row + 1) - margin)
                                    };
            }
            return cellCoordinates;
        }

        //public override Size GetPrefferedSize()
        //{
        //    return new System.Drawing.Size(40 * GetTrackerBoard().Columns, 40 * GetTrackerBoard().Rows);
        //}

        public override void InputMouseWheel(MouseEventArgs e, Size size)
        {
            if (selectedValueCell != null)
                if (e.Delta > 0)
                {
                    if (selectedValueCell.Value == null)
                        selectedValueCell.Value = false;
                    else if (selectedValueCell.Value == false)
                        selectedValueCell.Value = true;
                    else
                        selectedValueCell.Value = null;
                }
                else
                {
                    if (selectedValueCell.Value == null)
                        selectedValueCell.Value = true;
                    else if (selectedValueCell.Value == true)
                        selectedValueCell.Value = false;
                    else
                        selectedValueCell.Value = null;
                }

            this.OnRequestRefresh(EventArgs.Empty);
        }

        public override void InputMouse(MouseEventArgs e, Size s)
        {
            try
            {
                BoardTriddler b = this.GetTrackerBoard();
                KeyValuePair<Point, bool> p = GetBoardCoordinates(e, s, b);
                if (p.Key.X > -1 && p.Key.X < b.Rows && p.Key.Y > -1 && p.Key.Y < b.Rows) //if valid indexes
                {
                    CellValueTriddler pointedCell = (p.Value == true ? b.CellsMatrixRight /*Right Cell*/ : b.CellsMatrixLeft/*Left Cell*/ )[p.Key.X, p.Key.Y] as CellValueTriddler;
                    if (!b.InitialCells.Contains(pointedCell))
                        selectedValueCell = pointedCell;
                }
                else
                {
                    selectedValueCell = null;
                }
                this.OnRequestRefresh(EventArgs.Empty);
            }
            catch (Exception ex) { }
        }

        protected  KeyValuePair<Point,bool> GetBoardCoordinates(MouseEventArgs e, Size s, BoardTriddler b)
        {
            float floatWidthIndex = (float)e.X / ((float)s.Width / (float)b.Columns);
            float floatHeightIndex = (float)e.Y / ((float)s.Height / (float)b.Rows);

            return new KeyValuePair<Point, bool>(new Point((int)floatHeightIndex, (int)floatWidthIndex),
                ((floatWidthIndex - (int)(floatWidthIndex)) > (floatHeightIndex - (int)(floatHeightIndex)) ? true : false));
        }

        //public override void InputKey(KeyEventArgs e)
        //{
        //    BoardGriddler board = GetTrackerBoard();
        //    int numRequested = e.KeyValue - 49;
        //    if (selectedValueCell != null)
        //        if (numRequested > -1 && numRequested < 3)
        //            if (numRequested == 0)
        //                selectedValueCell.Value = null;
        //            else if (numRequested == 1)
        //                selectedValueCell.Value = true;
        //            else if (numRequested == 2)
        //                selectedValueCell.Value = false;
        //            else
        //                selectedValueCell.Value = null;
        //    this.OnRequestRefresh(EventArgs.Empty);
        //}

        //#region specific

        Common.Models.Griddler.CellValueGriddler selectedValueCell;

        //#endregion
    }
}
