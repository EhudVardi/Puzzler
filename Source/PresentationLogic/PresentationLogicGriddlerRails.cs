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

namespace PresentationLogic
{
    public class PresentationLogicGriddlerRails : PresentationLogicGriddler
    {
        public PresentationLogicGriddlerRails()
            : base()
        {
            this.LogicProxy = new LogicLayerGriddler();
        }

        public override void InitDisplay()
        {
            visualBoard = new VisualBoard();
            visualBoard.Init(this.GetSolvedBoard());
            margin = 1;
        }

        public override void Draw(object drawingContext, float width, float height)
        {
            try
            {
                //Graphics g = e.Graphics;

                float brWidth = width;
                float brHeight = height;

                float cellWidth = (float)brWidth / GetTrackerBoard().Columns;
                float cellHeight = (float)brHeight / GetTrackerBoard().Rows;

                Brush rowBrushBack = new SolidBrush(Color.FromArgb(16, Color.Red));
                Brush rowBrushFore = new SolidBrush(Color.FromArgb(224, Color.Red));

                Brush colBrushBack = new SolidBrush(Color.FromArgb(16, Color.Green));
                Brush colBrushFore = new SolidBrush(Color.FromArgb(224, Color.Green));

                if (this.visualBoard.SelectedRailGroup)
                {
                    DrawRows(drawingContext, cellWidth, cellHeight, rowBrushFore);
                    DrawColumns(drawingContext, cellWidth, cellHeight, colBrushBack);
                }
                else
                {
                    DrawColumns(drawingContext, cellWidth, cellHeight, colBrushFore);
                    DrawRows(drawingContext, cellWidth, cellHeight, rowBrushBack);
                }

            }
            catch (Exception)
            {
                //base.Draw(e);
            }
        }

        public override void InputMouseDown(MouseEventArgs e, Size s)
        {
            try
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        BoardGriddler b = this.GetTrackerBoard();
                        Point p = GetBoardCoordinates(e, s, b);

                        if (this.visualBoard.SelectedRailGroup) //rows
                        {
                            Rail r = visualBoard.RowRails[p.Y];
                            Car c = null;
                            for (int i = 0; i < r.Cars.Count; i++)
                            {
                                c = r.Cars[i];
                                if (p.X >= c.Position && p.X <= (c.Position + c.Size))
                                {
                                    break;
                                }
                            }

                            selectedRowCar = c;
                            selectedColumnCar = null;
                        }
                        else // columns
                        {
                            Rail r = visualBoard.ColumnRails[p.X];

                            Car c = null;
                            for (int i = 0; i < r.Cars.Count; i++)
                            {
                                c = r.Cars[i];
                                if (p.Y >= c.Position && p.Y <= (c.Position + c.Size))
                                {
                                    break;
                                }
                            }

                            selectedRowCar = null;
                            selectedColumnCar = c;
                        }

                        this.OnRequestRefresh(EventArgs.Empty);


                        break;

                    case MouseButtons.Middle:
                    case MouseButtons.None:
                    case MouseButtons.Right:
                    case MouseButtons.XButton1:
                    case MouseButtons.XButton2:
                    default:
                        break;
                }
            }
            catch (Exception ex) { }
        }
        public override void InputMouse(MouseEventArgs e, Size s)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:

                    try
                    {
                        BoardGriddler b = this.GetTrackerBoard();

                        int column = (int)((float)e.X / ((float)s.Width / (float)b.Columns));
                        int row = (int)((float)e.Y / ((float)s.Height / (float)b.Rows));

                        if (this.visualBoard.SelectedRailGroup) // rows
                        {
                            Rail r = this.visualBoard.RowRails[column];


                        }
                        else
                        {

                        }

                        this.OnRequestRefresh(EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {

                    }

                    break;
                case MouseButtons.Middle:
                case MouseButtons.None:
                case MouseButtons.Right:
                case MouseButtons.XButton1:
                case MouseButtons.XButton2:
                default:

                    this.visualBoard.SelectedRailGroup = !this.visualBoard.SelectedRailGroup;

                    this.selectedRowCar = null;
                    this.selectedColumnCar = null;
                  
                    break;
            }


            this.OnRequestRefresh(EventArgs.Empty);
        }
        public override void InputKey(KeyEventArgs e)
        {
            try
            {

                if (e.KeyValue == 88)
                {
                    if (this.visualBoard.SelectedRailGroup) //row
                    {
                        if (this.selectedRowCar != null) this.selectedRowCar.MoveForward();
                        //this.selectedRowCar.Position = Math.Max(0, Math.Min(this.selectedRowCar.OwnerRail.Size - this.selectedRowCar.Size, this.selectedRowCar.Position + 1));
                    }
                    else //column
                    {
                        if (this.selectedColumnCar != null) this.selectedColumnCar.MoveForward();
                        //this.selectedColumnCar.Position = Math.Max(0, Math.Min(this.selectedColumnCar.OwnerRail.Size - this.selectedColumnCar.Size, this.selectedColumnCar.Position + 1));
                    }
                }
                else if (e.KeyValue == 90)
                {
                    if (this.visualBoard.SelectedRailGroup) //row
                    {
                        if (this.selectedRowCar != null) this.selectedRowCar.MoveBackwards();
                        //this.selectedRowCar.Position = Math.Max(0, Math.Min(this.selectedRowCar.OwnerRail.Size - this.selectedRowCar.Size, this.selectedRowCar.Position - 1));
                    }
                    else //column
                    {
                        if (this.selectedColumnCar != null) this.selectedColumnCar.MoveBackwards();
                        //this.selectedColumnCar.Position = Math.Max(0, Math.Min(this.selectedColumnCar.OwnerRail.Size - this.selectedColumnCar.Size, this.selectedColumnCar.Position - 1));
                    }
                }

                OnRequestRefresh(EventArgs.Empty);
            }
            catch (Exception)
            {
            }
        }

        #region specific

        private void DrawColumns(object drawingContext, float cellWidth, float cellHeight, Brush colBrushFore)
        {
            for (int i = 0; i < this.visualBoard.ColumnRails.Count; i++)
            {
                Rail r = this.visualBoard.ColumnRails[i];
                for (int j = 0; j < r.Cars.Count; j++)
                {
                    Car c = r.Cars[j];

                    DrawCar(drawingContext, colBrushFore,
                        (int)(cellWidth * i + margin),
                        (int)(cellHeight * c.Position + margin),
                        (int)(cellWidth - 2 * +margin),
                        (int)(cellHeight * c.Size - 2 * margin));

                    if (c.Equals(selectedColumnCar))
                    {
                        DrawCar(drawingContext, Brushes.Yellow,
                        (int)(cellWidth * i + margin),
                        (int)(cellHeight * c.Position + margin),
                        (int)(cellWidth - 2 * +margin),
                        (int)(cellHeight * c.Size - 2 * margin));
                    }
                }
            }
        }
        private void DrawRows(object drawingContext, float cellWidth, float cellHeight, Brush rowBrushFore)
        {
            for (int i = 0; i < this.visualBoard.RowRails.Count; i++)
            {
                Rail r = this.visualBoard.RowRails[i];
                for (int j = 0; j < r.Cars.Count; j++)
                {
                    Car c = r.Cars[j];

                    DrawCar(drawingContext, rowBrushFore,
                        (int)(cellWidth * c.Position + margin),
                        (int)(cellHeight * i + margin),
                        (int)(cellWidth * c.Size - 2 * +margin),
                        (int)(cellHeight - 2 * margin));

                    if (c.Equals(selectedRowCar))
                    {
                        DrawCar(drawingContext, Brushes.Yellow,
                        (int)(cellWidth * c.Position + margin),
                        (int)(cellHeight * i + margin),
                        (int)(cellWidth * c.Size - 2 * +margin),
                        (int)(cellHeight - 2 * margin));
                    }

                }
            }
        }
        private void DrawCar(object drawingContext, Brush rowBrushFore, int x, int y, int w, int h)
        {
            try
            {
                OnRequestFillRectangle(drawingContext, rowBrushFore, x, y, w, h);
            }
            catch (Exception) { }
        }

        VisualBoard visualBoard;
        Car selectedRowCar = null;
        Car selectedColumnCar = null;

        public class VisualBoard
        {
            public List<Rail> RowRails { get; set; }
            public List<Rail> ColumnRails { get; set; }

            public bool SelectedRailGroup { get; set; }

            public void Init(BoardGriddler b)
            {
                RowRails = new List<Rail>();
                ColumnRails = new List<Rail>();

                SelectedRailGroup = true;

                foreach (GroupGriddler g in b.Groups)
                {
                    Rail rail = new Rail();
                    rail.Init(g);
                  
                    if (g is GroupGriddlerRow)
                    {
                        RowRails.Add(rail);
                    }
                    else if (g is GroupGriddlerColumn)
                    {
                        ColumnRails.Add(rail);
                    }
                }

            }
        }
        public class Rail
        {
            public List<Car> Cars { get; set; }
            public int Size { get; set; }

            internal void Init(GroupGriddler g)
            {
                Cars = new List<Car>();
                this.Size = g.Size;
                int movingPosition = 0;
                for (int i = 0; i < g.Numbers.Count; i++)
                {
                    Car car = new Car(this);
                    car.Size = g.Numbers[i];
                    car.Position = movingPosition;
                    Cars.Add(car);

                    movingPosition += car.Size + 1;
                }
            }

            public override string ToString()
            {
                string bla = "";
                foreach (Car c in this.Cars)
                {
                    bla += c.Size + ", ";
                }
                return bla;
            }
        }
        public class Car
        {
            public Rail OwnerRail { get; set; }
            public int Position { get; set; }
            public int Size { get; set; }

            public Car(Rail ownerRail)
            {
                this.OwnerRail = ownerRail;

            }

            public void MoveForward()
            {
                int i = GetIndexInRail();

                bool canMove = true;
                int x;
                for (x = i; x < this.OwnerRail.Cars.Count; x++)
                {
                    if (x == this.OwnerRail.Cars.Count - 1)
                    {
                        if (this.OwnerRail.Cars[x].Position + this.OwnerRail.Cars[x].Size >= this.OwnerRail.Size)
                            canMove = false;
                    }
                    else
                    {
                        if (this.OwnerRail.Cars[x].Position + this.OwnerRail.Cars[x].Size < this.OwnerRail.Cars[x + 1].Position - 1)
                            break;
                    }
                }

                if (canMove)
                {
                    if (x == this.OwnerRail.Cars.Count)
                    {
                        for (int j = i; j < x; j++)
                        {
                            this.OwnerRail.Cars[j].Position++;
                        }
                    }
                    else
                    {
                        for (int j = i; j <= x; j++)
                        {
                            this.OwnerRail.Cars[j].Position++;
                        }
                    }
                }

            }
            public void MoveBackwards()
            {
                int i = GetIndexInRail();

                bool canMove = true;
                int x;
                for (x = i; x >= 0; x--)
                {
                    if (x == 0)
                    {
                        if (this.OwnerRail.Cars[x].Position == 0)
                            canMove = false;
                    }
                    else
                    {
                        if (this.OwnerRail.Cars[x - 1].Position + this.OwnerRail.Cars[x - 1].Size < this.OwnerRail.Cars[x].Position - 1)
                            break;
                    }
                }

                if (canMove)
                {
                    if (x == -1)
                    {
                        for (int j = i; j > x; j--)
                        {
                            this.OwnerRail.Cars[j].Position--;
                        }
                    }
                    else
                    {
                        for (int j = i; j >= x; j--)
                        {
                            this.OwnerRail.Cars[j].Position--;
                        }
                    }

                }
            }

            public int GetIndexInRail()
            {
                return this.OwnerRail.Cars.IndexOf(this);
            }
        }

        #endregion
    }
}
