using System;
using Logic;
using Data.DataModels;
using Common.Models.Sudoku;
using PresentationLogic.Rendering;

namespace PresentationLogic
{
    public class PresentationLogicSudoku : PresentationLogicGeneric<SudokuPuzzle, BoardSudoku>
    {
        public PresentationLogicSudoku()
        {
            this.LogicProxy = new LogicLayerSudoku();
            this.URL = "http://www.sudokuconquest.com/9x9/expert";
        }

        public override void DrawBoard(BoardSudoku trackerBoard, BoardSudoku solvedBoard, float width, float height)
        {
            float cellWidth  = width  / trackerBoard.Columns;
            float cellHeight = height / trackerBoard.Rows;

            float widthB  = width  / trackerBoard.N;
            float heightB = height / trackerBoard.M;

            for (int i = 0; i < trackerBoard.CellsMatrix.GetLength(0); i++)
                DrawLine(PuzzlerColor.Black, 1, 0, cellHeight * i, width, cellHeight * i);

            for (int j = 0; j < trackerBoard.CellsMatrix.GetLength(1); j++)
                DrawLine(PuzzlerColor.Black, 1, cellWidth * j, 0, cellWidth * j, height);

            float marginBoxes = margin / 4;
            for (int i = 0; i < trackerBoard.N; i++)
                for (int j = 0; j < trackerBoard.M; j++)
                    DrawRect(PuzzlerColor.Black, 1,
                        widthB * j + marginBoxes, heightB * i + marginBoxes,
                        widthB - marginBoxes * 2f, heightB - marginBoxes * 2f);

            foreach (CellValueSudoku valueCell in trackerBoard.ValueCells)
            {
                CellValueSudoku solvedValueCell = solvedBoard.CellsMatrix[valueCell.Row, valueCell.Column] as CellValueSudoku;

                PuzzlerColor backColor;
                PuzzlerColor foreColor;

                if (!trackerBoard.InitialCells.Contains(valueCell))
                { backColor = bNull; foreColor = bCorrect; }
                else
                { backColor = bFixed; foreColor = bText; }

                FillRect(backColor,
                    cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);

                switch (this.displayType)
                {
                    case DisplayType.Board:
                        if (valueCell.IsFixed)
                            DrawText((valueCell.Value + 1).ToString(), font, foreColor,
                                cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                                cellWidth - margin * 2f, cellHeight - margin * 2f);
                        break;
                    case DisplayType.Hint:
                        if (valueCell.IsFixed)
                        {
                            PuzzlerColor textColor = (selectedValueCell != null
                                && selectedValueCell.Value.HasValue && valueCell.Value.HasValue
                                && selectedValueCell.Value == valueCell.Value)
                                ? bIncorrect : foreColor;

                            DrawText((valueCell.Value + 1).ToString(), font, textColor,
                                cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                                cellWidth - margin * 2f, cellHeight - margin * 2f);
                        }
                        break;
                    case DisplayType.Solution:
                        DrawText((solvedValueCell.Value + 1).ToString(), font, foreColor,
                            cellWidth * valueCell.Column + margin, cellHeight * valueCell.Row + margin,
                            cellWidth - margin * 2f, cellHeight - margin * 2f);
                        break;
                }
            }

            if (selectedValueCell != null)
                DrawRect(PuzzlerColor.Black, margin,
                    cellWidth * selectedValueCell.Column + margin, cellHeight * selectedValueCell.Row + margin,
                    cellWidth - margin * 2f, cellHeight - margin * 2f);
        }

        public override (int Width, int Height) GetPrefferedSize()
        {
            return (40 * GetTrackerBoard().Columns, 40 * GetTrackerBoard().Rows);
        }

        public override void HandlePointer(PointerEvent e, float sizeX, float sizeY)
        {
            try
            {
                BoardSudoku b  = this.GetTrackerBoard();
                int column = (int)(e.X / (sizeX / b.Columns));
                int row    = (int)(e.Y / (sizeY / b.Rows));
                CellValueSudoku pointedCell = b.CellsMatrix[row, column] as CellValueSudoku;
                if (!b.InitialCells.Contains(pointedCell))
                    selectedValueCell = pointedCell;
                this.OnRequestRefresh(EventArgs.Empty);
            }
            catch (Exception) { }
        }

        public override void HandleKey(KeyEvent e)
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

        public override void HandlePointerWheel(PointerEvent e, float sizeX, float sizeY)
        {
            if (selectedValueCell != null)
            {
                if (!selectedValueCell.Value.HasValue)
                {
                    selectedValueCell.Value = 0;
                }
                else
                {
                    int maxValue  = GetTrackerBoard().N * GetTrackerBoard().M;
                    int nextValue = ((int)selectedValueCell.Value + (e.Delta > 0 ? 1 : -1)) % maxValue;
                    selectedValueCell.Value = nextValue < 0 ? nextValue + maxValue : nextValue;
                }
            }
            this.OnRequestRefresh(EventArgs.Empty);
        }

        private CellValueSudoku selectedValueCell;
    }
}
