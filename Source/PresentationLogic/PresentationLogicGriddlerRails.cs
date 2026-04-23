using System;
using System.Collections.Generic;
using Logic;
using Data.DataModels;
using Common.Models.Griddler;
using PresentationLogic.Rendering;

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

        public override void DrawBoard(BoardGriddler trackerBoard, BoardGriddler solvedBoard, float width, float height)
        {
            try
            {
                float cellWidth  = width  / trackerBoard.Columns;
                float cellHeight = height / trackerBoard.Rows;

                PuzzlerColor rowBrushBack = PuzzlerColor.Red.WithAlpha(16);
                PuzzlerColor rowBrushFore = PuzzlerColor.Red.WithAlpha(224);
                PuzzlerColor colBrushBack = PuzzlerColor.Green.WithAlpha(16);
                PuzzlerColor colBrushFore = PuzzlerColor.Green.WithAlpha(224);

                if (this.visualBoard.SelectedRailGroup)
                {
                    DrawRows(cellWidth, cellHeight, rowBrushFore);
                    DrawColumns(cellWidth, cellHeight, colBrushBack);
                }
                else
                {
                    DrawColumns(cellWidth, cellHeight, colBrushFore);
                    DrawRows(cellWidth, cellHeight, rowBrushBack);
                }
            }
            catch (Exception) { }
        }

        public override void HandlePointerDown(PointerEvent e, float sizeX, float sizeY)
        {
            try
            {
                if (e.Button == PointerButton.Left)
                {
                    BoardGriddler b = this.GetTrackerBoard();
                    var (row, col) = GetBoardCoordinates(e, sizeX, sizeY, b);

                    if (this.visualBoard.SelectedRailGroup)
                    {
                        Rail r = visualBoard.RowRails[col];
                        Car c = null;
                        for (int i = 0; i < r.Cars.Count; i++)
                        {
                            c = r.Cars[i];
                            if (row >= c.Position && row <= (c.Position + c.Size))
                                break;
                        }
                        selectedRowCar    = c;
                        selectedColumnCar = null;
                    }
                    else
                    {
                        Rail r = visualBoard.ColumnRails[row];
                        Car c = null;
                        for (int i = 0; i < r.Cars.Count; i++)
                        {
                            c = r.Cars[i];
                            if (col >= c.Position && col <= (c.Position + c.Size))
                                break;
                        }
                        selectedRowCar    = null;
                        selectedColumnCar = c;
                    }

                    this.OnRequestRefresh(EventArgs.Empty);
                }
            }
            catch (Exception) { }
        }

        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            if (e.Button == PointerButton.Left)
            {
                try
                {
                    BoardGriddler b = this.GetTrackerBoard();
                    int column = (int)(e.X / (sizeX / b.Columns));
                    int row    = (int)(e.Y / (sizeY / b.Rows));

                    if (this.visualBoard.SelectedRailGroup)
                    {
                        // row rail selected — placeholder for drag logic
                    }

                    this.OnRequestRefresh(EventArgs.Empty);
                }
                catch (Exception) { }
            }
            else
            {
                this.visualBoard.SelectedRailGroup = !this.visualBoard.SelectedRailGroup;
                this.selectedRowCar    = null;
                this.selectedColumnCar = null;
            }

            this.OnRequestRefresh(EventArgs.Empty);
        }

        public override void HandleKey(KeyEvent e)
        {
            try
            {
                if (e.KeyValue == 88)
                {
                    if (this.visualBoard.SelectedRailGroup)
                    { if (this.selectedRowCar    != null) this.selectedRowCar.MoveForward(); }
                    else
                    { if (this.selectedColumnCar != null) this.selectedColumnCar.MoveForward(); }
                }
                else if (e.KeyValue == 90)
                {
                    if (this.visualBoard.SelectedRailGroup)
                    { if (this.selectedRowCar    != null) this.selectedRowCar.MoveBackwards(); }
                    else
                    { if (this.selectedColumnCar != null) this.selectedColumnCar.MoveBackwards(); }
                }

                OnRequestRefresh(EventArgs.Empty);
            }
            catch (Exception) { }
        }

        private void DrawColumns(float cellWidth, float cellHeight, PuzzlerColor color)
        {
            for (int i = 0; i < this.visualBoard.ColumnRails.Count; i++)
            {
                Rail r = this.visualBoard.ColumnRails[i];
                for (int j = 0; j < r.Cars.Count; j++)
                {
                    Car c = r.Cars[j];
                    DrawCar(color,
                        (int)(cellWidth * i + margin), (int)(cellHeight * c.Position + margin),
                        (int)(cellWidth - 2 * margin), (int)(cellHeight * c.Size - 2 * margin));

                    if (c.Equals(selectedColumnCar))
                        DrawCar(PuzzlerColor.Yellow,
                            (int)(cellWidth * i + margin), (int)(cellHeight * c.Position + margin),
                            (int)(cellWidth - 2 * margin), (int)(cellHeight * c.Size - 2 * margin));
                }
            }
        }

        private void DrawRows(float cellWidth, float cellHeight, PuzzlerColor color)
        {
            for (int i = 0; i < this.visualBoard.RowRails.Count; i++)
            {
                Rail r = this.visualBoard.RowRails[i];
                for (int j = 0; j < r.Cars.Count; j++)
                {
                    Car c = r.Cars[j];
                    DrawCar(color,
                        (int)(cellWidth * c.Position + margin), (int)(cellHeight * i + margin),
                        (int)(cellWidth * c.Size - 2 * margin), (int)(cellHeight - 2 * margin));

                    if (c.Equals(selectedRowCar))
                        DrawCar(PuzzlerColor.Yellow,
                            (int)(cellWidth * c.Position + margin), (int)(cellHeight * i + margin),
                            (int)(cellWidth * c.Size - 2 * margin), (int)(cellHeight - 2 * margin));
                }
            }
        }

        private void DrawCar(PuzzlerColor color, int x, int y, int w, int h)
        {
            try { FillRect(color, x, y, w, h); }
            catch (Exception) { }
        }

        private VisualBoard visualBoard;
        private Car selectedRowCar    = null;
        private Car selectedColumnCar = null;

        public class VisualBoard
        {
            public List<Rail> RowRails    { get; set; }
            public List<Rail> ColumnRails { get; set; }
            public bool SelectedRailGroup { get; set; }

            public void Init(BoardGriddler b)
            {
                RowRails    = new List<Rail>();
                ColumnRails = new List<Rail>();
                SelectedRailGroup = true;

                foreach (GroupGriddler g in b.Groups)
                {
                    Rail rail = new Rail();
                    rail.Init(g);
                    if      (g is GroupGriddlerRow)    RowRails.Add(rail);
                    else if (g is GroupGriddlerColumn) ColumnRails.Add(rail);
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
                    car.Size     = g.Numbers[i];
                    car.Position = movingPosition;
                    Cars.Add(car);
                    movingPosition += car.Size + 1;
                }
            }

            public override string ToString()
            {
                string bla = "";
                foreach (Car c in this.Cars) bla += c.Size + ", ";
                return bla;
            }
        }

        public class Car
        {
            public Rail OwnerRail { get; set; }
            public int Position   { get; set; }
            public int Size       { get; set; }

            public Car(Rail ownerRail) { this.OwnerRail = ownerRail; }

            public void MoveForward()
            {
                int i = GetIndexInRail();
                bool canMove = true;
                int x;
                for (x = i; x < this.OwnerRail.Cars.Count; x++)
                {
                    if (x == this.OwnerRail.Cars.Count - 1)
                    { if (this.OwnerRail.Cars[x].Position + this.OwnerRail.Cars[x].Size >= this.OwnerRail.Size) canMove = false; }
                    else
                    { if (this.OwnerRail.Cars[x].Position + this.OwnerRail.Cars[x].Size < this.OwnerRail.Cars[x + 1].Position - 1) break; }
                }

                if (canMove)
                {
                    int end = (x == this.OwnerRail.Cars.Count) ? x - 1 : x;
                    for (int j = i; j <= end; j++) this.OwnerRail.Cars[j].Position++;
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
                    { if (this.OwnerRail.Cars[x].Position == 0) canMove = false; }
                    else
                    { if (this.OwnerRail.Cars[x - 1].Position + this.OwnerRail.Cars[x - 1].Size < this.OwnerRail.Cars[x].Position - 1) break; }
                }

                if (canMove)
                {
                    int end = (x == -1) ? 0 : x;
                    for (int j = i; j >= end; j--) this.OwnerRail.Cars[j].Position--;
                }
            }

            public int GetIndexInRail() => this.OwnerRail.Cars.IndexOf(this);
        }
    }
}
