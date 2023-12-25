using System;
using System.Collections.Generic;
using System.Text;
using Logic.Sudoku;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Data.DataModels;
using Data;
using Logic;
using Common.Models.Sudoku;

namespace PresentationLogic
{
    public class PresentationLogicSudoku : PresentationLogicGeneric<SudokuPuzzle, BoardSudoku>
    {
        public PresentationLogicSudoku()
        {
            this.LogicProxy = new LogicLayerSudoku();
            this.URL = "http://www.sudokuconquest.com/9x9/expert";
        }

        public override void DrawBoard(BoardSudoku trackerBoard, BoardSudoku solvedBoard, object drawingContext, float width, float height)
        {
            float brWidth = width;
            float brHeight = height;

            float cellWidth = (float)brWidth / trackerBoard.Columns;
            float cellHeight = (float)brHeight / trackerBoard.Rows;


            ////draw board groups frame
            float widthB = (float)brWidth / trackerBoard.N;
            float heightB = (float)brHeight / trackerBoard.M;

            //rows 
            for (int i = 0; i < trackerBoard.CellsMatrix.GetLength(0); i++)
                OnRequestDrawLine(drawingContext,
                Pens.Black, 0, cellHeight * i, (float)brWidth, cellHeight * i);

            //columns
            for (int j = 0; j < trackerBoard.CellsMatrix.GetLength(1); j++)
                OnRequestDrawLine(drawingContext,
                Pens.Black, cellWidth * j, 0, cellWidth * j, (float)brHeight);

            //boxes
            float marginBoxes = margin / 4;
            for (int i = 0; i < trackerBoard.N; i++)
                for (int j = 0; j < trackerBoard.M; j++)
                    OnRequestDrawRectangle(drawingContext,
                    Pens.Black, widthB * j + marginBoxes, heightB * i + marginBoxes, widthB - marginBoxes * 2f, heightB - marginBoxes * 2f);

            //draw cells
            foreach (CellValueSudoku valueCell in trackerBoard.ValueCells)
            {
                CellValueSudoku solvedValueCell = solvedBoard.CellsMatrix[valueCell.Row, valueCell.Column] as CellValueSudoku;


                Brush brushBackColor;
                Brush brushForeColor;

                if (!trackerBoard.InitialCells.Contains(valueCell))
                { brushBackColor = bNull; brushForeColor = bCorrect; }
                else
                { brushBackColor = bFixed; brushForeColor = bText; }

                //draw value cell back color (initial or not)
                OnRequestFillRectangle(drawingContext,
                    brushBackColor,
                    cellWidth * valueCell.Column + margin,
                    cellHeight * valueCell.Row + margin,
                    cellWidth - margin * 2f,
                    cellHeight - margin * 2f
                    );


                //draw fixed cells values
                //draw value cell value
                switch (this.displayType)
                {
                    case DisplayType.Board:
                        if (valueCell.IsFixed)
                        {
                            OnRequestDrawText(drawingContext,
                                (valueCell.Value + 1).ToString(), font, brushForeColor,
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

                            // number color depend on the selected cell number
                            if (selectedValueCell != null && selectedValueCell.Value.HasValue == true && valueCell.Value.HasValue == true && selectedValueCell.Value == valueCell.Value)
                                brushValue = bIncorrect;
                            else
                                brushValue = brushForeColor;

                            OnRequestDrawText(drawingContext,
                                (valueCell.Value + 1).ToString(), font, brushValue,
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
                            (solvedValueCell.Value + 1).ToString(), font, brushForeColor,
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
                BoardSudoku b = this.GetTrackerBoard();
                int column = (int)((float)e.X / ((float)s.Width / (float)b.Columns));
                int row = (int)((float)e.Y / ((float)s.Height / (float)b.Rows));
                CellValueSudoku pointedCell = b.CellsMatrix[row, column] as CellValueSudoku;
                if (!b.InitialCells.Contains(pointedCell))
                    selectedValueCell = pointedCell;
                this.OnRequestRefresh(EventArgs.Empty);
            }
            catch (Exception ex) { }
        }

        public override void InputKey(KeyEventArgs e)
        {
            try
            {
                BoardSudoku board = GetTrackerBoard();
                int numRequested = e.KeyValue - 49;
                if (selectedValueCell != null)
                    if (numRequested > -1 && numRequested < board.Size)
                        selectedValueCell.Value = numRequested;
                    else
                        selectedValueCell.Value = null;
                this.OnRequestRefresh(EventArgs.Empty);
            }
            catch (Exception) { }
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
                    int maxValue = GetTrackerBoard().N * GetTrackerBoard().M;
                    int nextValue = ((int)(selectedValueCell.Value) + (e.Delta > 0 ? 1 : -1)) % maxValue;
                    nextValue = nextValue < 0 ? nextValue + maxValue : nextValue;
                    selectedValueCell.Value = nextValue;
                }
            this.OnRequestRefresh(EventArgs.Empty);
        }

        #region specific

        CellValueSudoku selectedValueCell;

        #endregion
    }
}



/*
List<Bitmap> numbersImages;
Bitmap voidImage;
int imageSize = 100;


public void InitImages(int n)
{
    voidImage = new Bitmap(imageSize, imageSize, PixelFormat.Format32bppArgb);

    numbersImages = new List<Bitmap>();

    for (int i = 0; i < n; i++)
    {
        numbersImages.Add(CreateImage(i + 1, 255));
    }
}

private Bitmap CreateImage(int num, int transparency)
{
    Bitmap bmp = new Bitmap(imageSize, imageSize, PixelFormat.Format32bppArgb);

    float fontSize = (float)imageSize / 2f;

    Font f = new Font(FontFamily.GenericSansSerif, (int)fontSize, GraphicsUnit.Pixel);

    StringFormat sf = new StringFormat();
    sf.Alignment = StringAlignment.Center;
    sf.LineAlignment = StringAlignment.Center;

    Graphics g = Graphics.FromImage(bmp);


    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
    g.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Orange)), new Rectangle(0, 0, imageSize, imageSize));
    g.DrawString(num.ToString(), f, new SolidBrush(Color.FromArgb(transparency, Color.Red)), new RectangleF(0, 0, imageSize, imageSize), sf);
    g.Flush();

    return bmp;
}
*/
