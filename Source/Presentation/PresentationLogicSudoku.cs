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

namespace Presentation
{
    public class PresentationLogicSudoku : PresentationLogicGeneric<SudokuPuzzle, BoardSudoku>
    {
        public PresentationLogicSudoku()
        {
            this.LogicProxy = new LogicLayerSudoku();

            this.URL = "http://www.sudokuconquest.com/9x9/expert";
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


                //draw cells
                foreach (CellValueSudoku valueCell in GetTrackerBoard().ValueCells)
                {
                    CellValueSudoku solvedValueCell = GetSolvedBoard().CellsMatrix[valueCell.Row, valueCell.Column] as CellValueSudoku;


                    Brush brushBackColor;

                    if (!GetTrackerBoard().InitialCells.Contains(valueCell))
                        brushBackColor = bNull;
                    else brushBackColor = bFixed;

                    //draw value cell back color (initial or not)
                    g.FillRectangle(brushBackColor,
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




                //draw board groups frame
                float widthB = (float)brWidth / GetTrackerBoard().N;
                float heightB = (float)brHeight / GetTrackerBoard().M;

                //rows 
                for (int i = 0; i < GetTrackerBoard().CellsMatrix.GetLength(0); i++)
                    g.DrawLine(Pens.Black, 0, cellHeight * i, (float)brWidth, cellHeight * i);

                //columns
                for (int j = 0; j < GetTrackerBoard().CellsMatrix.GetLength(1); j++)
                    g.DrawLine(Pens.Black, cellWidth * j, 0, cellWidth * j, (float)brHeight);

                //boxes
                for (int i = 0; i < GetTrackerBoard().N; i++)
                    for (int j = 0; j < GetTrackerBoard().M; j++)
                        g.DrawRectangle(Pens.Black, widthB * j + margin, heightB * i + margin, widthB - margin * 2f, heightB - margin * 2f);




            }
            catch (Exception ex)
            {
                base.Draw(e);
            }
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
            catch (Exception ex)
            {

            }
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
            catch (Exception)
            {
            }
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
