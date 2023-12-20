using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Data.DataModels;
using Common.Models.Base;
using Common.Models.Kakuru;

namespace Logic.Kakuru
{
    public class FactoryKakuru : FactoryGeneric<PuzzleKakuru, BoardKakuru>
    {

        public override BoardKakuru GenerateRandom()
        {
            return GenerateRandom(9, 9);
        }



        protected override BoardKakuru CreateBoardFromPuzzleObject(PuzzleKakuru puzzle)
        {
            BoardKakuru board = new BoardKakuru();

            board.NumberRange = puzzle.NumberRange;

            board.CellsMatrix = new CellBase[puzzle.Rows, puzzle.Columns];
            board.Groups = new List<GroupKakuru>();

            for (int i = 0; i < puzzle.Rows; i++)
            {
                for (int j = 0; j < puzzle.Columns; j++)
                {
                    board.CellsMatrix[i, j] = new CellGroupHolderKakuru(i, j);
                }
            }

            foreach (DefinedGroupKakuru line in puzzle.SumLines)
            {
                if (line.HorizontalVertical)//down
                {
                    for (int i = 0; i < line.Size; i++)
                    {
                        CellValueKakuru fillCell = new CellValueKakuru(line.RowI + i + 1, line.ColumnI);
                        board.CellsMatrix[fillCell.Row, fillCell.Column] = fillCell;
                    }
                }
                else //right
                {
                    for (int j = 0; j < line.Size; j++)
                    {
                        CellValueKakuru fillCell = new CellValueKakuru(line.RowI, line.ColumnI + j + 1);
                        board.CellsMatrix[fillCell.Row, fillCell.Column] = fillCell;
                    }
                }
            }

            foreach (DefinedGroupKakuru line in puzzle.SumLines)
            {
                CellGroupHolderKakuru lineCell = board.CellsMatrix[line.RowI, line.ColumnI] as CellGroupHolderKakuru;

                lineCell.Row = line.RowI;
                lineCell.Column = line.ColumnI;
                if (line.HorizontalVertical)//down
                {
                    GroupKakuru downGroup = new GroupKakuru();

                    downGroup.RightDown = true;
                    downGroup.Sum = line.Sum;
                    downGroup.Cells = new List<CellValueKakuru>();
                    for (int i = 0; i < line.Size; i++)
                    {
                        CellValueKakuru fillCell = board.CellsMatrix[line.RowI + i + 1, line.ColumnI] as CellValueKakuru;
                        downGroup.Cells.Add(fillCell);
                        fillCell.Groups.Add(downGroup);
                    }

                    lineCell.DownGroup = downGroup;
                    board.Groups.Add(downGroup);
                }
                else //right
                {
                    GroupKakuru rightGroup = new GroupKakuru();

                    rightGroup.RightDown = false;
                    rightGroup.Sum = line.Sum;
                    rightGroup.Cells = new List<CellValueKakuru>();
                    for (int j = 0; j < line.Size; j++)
                    {
                        CellValueKakuru fillCell = board.CellsMatrix[line.RowI, line.ColumnI + j + 1] as CellValueKakuru;
                        rightGroup.Cells.Add(fillCell);
                        fillCell.Groups.Add(rightGroup);
                    }

                    lineCell.RightGroup = rightGroup;
                    board.Groups.Add(rightGroup);
                }

            }

            foreach (FixedCellKakuru fixedCell in puzzle.FixedCells)
            {
                CellValueKakuru fillCell = board.CellsMatrix[fixedCell.Row, fixedCell.Column] as CellValueKakuru;
                if (fillCell != null)
                {
                    fillCell.Value = fixedCell.Value;
                    board.InitialCells.Add(fillCell);
                }
                else
                {
                    throw new Exception();
                }
            }


            return board;
        }

        protected override PuzzleKakuru CreatePuzzleObjectFromBoard(BoardKakuru board)
        {
            PuzzleKakuru puzzle = new PuzzleKakuru();

            puzzle.Rows = board.Rows;
            puzzle.Columns = board.Columns;

            puzzle.NumberRange = board.NumberRange;

            puzzle.SumLines = new List<DefinedGroupKakuru>();

            puzzle.FixedCells = new List<FixedCellKakuru>();

            foreach (CellBase cell in board.CellsMatrix)
            {
                CellGroupHolderKakuru lineCell = cell as CellGroupHolderKakuru;

                if (lineCell != null)
                {
                    GroupKakuru downGroup = lineCell.DownGroup;
                    GroupKakuru rightGroup = lineCell.RightGroup;

                    if (downGroup != null)
                    {
                        DefinedGroupKakuru sumLine = new DefinedGroupKakuru();
                        sumLine.RowI = lineCell.Row;
                        sumLine.ColumnI = lineCell.Column;
                        sumLine.HorizontalVertical = true;
                        sumLine.Size = downGroup.Size;
                        sumLine.Sum = downGroup.Sum;

                        puzzle.SumLines.Add(sumLine);
                    }

                    if (rightGroup != null)
                    {
                        DefinedGroupKakuru sumLine = new DefinedGroupKakuru();
                        sumLine.RowI = lineCell.Row;
                        sumLine.ColumnI = lineCell.Column;
                        sumLine.HorizontalVertical = false;
                        sumLine.Size = rightGroup.Size;
                        sumLine.Sum = rightGroup.Sum;

                        puzzle.SumLines.Add(sumLine);
                    }

                }
            }

            foreach (CellBase cell in board.CellsMatrix)
            {
                CellValueKakuru fillCell = cell as CellValueKakuru;

                if (fillCell != null)
                {
                    if (fillCell.Value != null)
                        puzzle.FixedCells.Add(new FixedCellKakuru(fillCell.Row, fillCell.Column, (int)fillCell.Value));

                }
            }

            return puzzle;
        }



        private static BoardKakuru GenerateRandom(int rows, int columns)
        {

            //KakuruBoard kakuruBoard = GenerateRandomStructuredMatrix(rows, columns, numbersRange);
            //KakuruBoard kakuruBoard = GenerateRandomStructuredMatrixAngle(rows, columns, numbersRange);
            //KakuruBoard kakuruBoard = GenerateRandomStructuredMatrixLine(rows, columns, numbersRange);
            BoardKakuru kakuruBoard = GenerateRandomStructuredMatrixPlusSignLogicSymmetrical(rows, columns);
            
            LinkMatrixCells(kakuruBoard);

            //RandomlySetAllFillCellsValuesRandomPlacement(kakuruBoard);
            while (!RandomlySetAllFillCellsValuesByDifficulty(kakuruBoard, 0.9f)) ;


            FixingAllMustCells2(kakuruBoard);

            return kakuruBoard;
        }

        /*
        private static BoardKakuru GenerateRandomStructuredMatrix(int rows, int columns, List<int> numbersRange)
        {
            BoardKakuru kakuruBoard = new BoardKakuru();

            kakuruBoard.NumberRange = numbersRange;

            ////Set random Structure
            Random rand = new Random();

            //create a temporary larger matrix to keep logic simple (out of bounds)
            int rowsTemp = rows + 3, columnsTemp = columns + 3;
            CellBase[,] tempMat = new CellBase[rowsTemp, columnsTemp];

            //set two first and last rows and columns to be line cells
            for (int i = 0; i < rowsTemp; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    CellKakuruLine lineCell = new CellKakuruLine();
                    lineCell.Row = i;
                    lineCell.Column = j;
                    tempMat[i, j] = lineCell;

                    CellKakuruLine lineCellR = new CellKakuruLine();
                    lineCellR.Row = i;
                    lineCellR.Column = columnsTemp - 1 - j;
                    tempMat[i, columnsTemp - 1 - j] = lineCellR;
                }
            }
            for (int j = 0; j < columnsTemp; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    CellKakuruLine lineCell = new CellKakuruLine();
                    lineCell.Row = i;
                    lineCell.Column = j;
                    tempMat[i, j] = lineCell;

                    CellKakuruLine lineCellB = new CellKakuruLine();
                    lineCellB.Row = rowsTemp - 1 - i;
                    lineCellB.Column = j;
                    tempMat[rowsTemp - 1 - i, j] = lineCellB;
                }
            }

            //randomly try to put line cells in the inner rectangle that's left, keeping the logic rule true
            int totalPossibleFillCells = (tempMat.GetLength(0) - 4) * (tempMat.GetLength(1) - 4);
            float ratio = 0.4f;

            int lineCellsToPut = (int)(ratio * (float)totalPossibleFillCells);
            int lineCellsPuttedCount = 0;
            while (lineCellsPuttedCount < lineCellsToPut)
            {
                int randRow = rand.Next(2, tempMat.GetLength(0) - 2);
                int randColumn = rand.Next(2, tempMat.GetLength(1) - 2);

                if (tempMat[randRow, randColumn] == null)
                {
                    if (tempMat[randRow - 1, randColumn] == null && tempMat[randRow - 2, randColumn] != null) { } //up is bad
                    else if (tempMat[randRow + 1, randColumn] == null && tempMat[randRow + 2, randColumn] != null) { } //down is bad
                    else if (tempMat[randRow, randColumn - 1] == null && tempMat[randRow, randColumn - 2] != null) { }//left is bad 
                    else if (tempMat[randRow, randColumn + 1] == null && tempMat[randRow, randColumn + 2] != null) { }//right is bad
                    else
                    {
                        //all sides are ok to put a line cell
                        CellKakuruLine lineCell = new CellKakuruLine();
                        lineCell.Row = randRow;
                        lineCell.Column = randColumn;
                        tempMat[randRow, randColumn] = lineCell;

                        lineCellsPuttedCount++;
                    }
                }
            }


            //fill in the rest of the cells to be fill cells
            for (int i = 0; i < rowsTemp; i++)
            {
                for (int j = 0; j < columnsTemp; j++)
                {
                    if (tempMat[i, j] == null)
                    {
                        CellKakuruFill fillCell = new CellKakuruFill();
                        fillCell.Row = i;
                        fillCell.Column = j;

                        tempMat[i, j] = fillCell;
                    }
                }
            }

            //creating our original matrix according to the temporary matrix, as a sub matrix

            CellBase[,] finalMatrix = new CellBase[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    finalMatrix[i, j] = tempMat[i + 1, j + 1];
                    finalMatrix[i, j].Row -= 1;
                    finalMatrix[i, j].Column -= 1;
                }
            }

            kakuruBoard.Matrix = finalMatrix;

            return kakuruBoard;
            ////
        }

        private static BoardKakuru GenerateRandomStructuredMatrixAngle(int rows, int columns, List<int> numbersRange)
        {
            BoardKakuru kakuruBoard = new BoardKakuru();

            kakuruBoard.NumberRange = numbersRange;

            ////Set random Structure
            Random rand = new Random();

            //create the matrix rows*columns
            kakuruBoard.Matrix = new CellBase[rows, columns];

            //set first row and column to line cells
            for (int i = 0; i < rows; i++)
            {
                CellKakuruLine lineCell = new CellKakuruLine();
                lineCell.Row = i;
                lineCell.Column = 0;
                kakuruBoard.Matrix[i, 0] = lineCell;
            }
            for (int j = 0; j < columns; j++)
            {
                CellKakuruLine lineCell = new CellKakuruLine();
                lineCell.Row = 0;
                lineCell.Column = j;
                kakuruBoard.Matrix[0, j] = lineCell;
            }

            //1. randomize a point in the subMatrix as a start point
            Point start = new Point(1,1);
            Point end = new Point(rows - 1, columns - 1);

            Point axis = new Point(rand.Next(start.X, end.X), rand.Next(start.Y, end.Y));
            int nullCellCounter = 0;
            float difficutlyRatio = 0.4f;

            //2. do:
            do
            {
                bool rightLeft; // false -> right
                bool downUp; // false -> down
                //2.1 if point is at the edges of the subMatrix then select up or down and left or right accordingly
                if (axis.X == start.X)
                    rightLeft = false; //choose right
                else if (axis.X == end.X)
                    rightLeft = true; //choose left
                else
                    rightLeft = rand.NextDouble() > 0.5 ? false : true;//randomize left right choice


                if (axis.Y == start.Y)
                    downUp = false; //choose down
                else if (axis.Y == end.Y)
                    downUp = true; //choose up
                else
                    downUp = rand.NextDouble() > 0.5 ? false : true;//randomize down right up

                //2.2 set the randomized shape as fill cells
                CellKakuruFill fillCellC = new CellKakuruFill();
                fillCellC.Row = axis.Y;
                fillCellC.Column = axis.X;

                CellKakuruFill fillCellH = new CellKakuruFill();
                fillCellH.Row = axis.Y;
                fillCellH.Column = axis.X + (rightLeft ? -1 : 1);

                CellKakuruFill fillCellV = new CellKakuruFill();
                fillCellV.Row = axis.Y + (downUp ? -1 : 1);
                fillCellV.Column = axis.X;

                kakuruBoard.Matrix[fillCellC.Row, fillCellC.Column] = fillCellC;
                kakuruBoard.Matrix[fillCellH.Row, fillCellH.Column] = fillCellH;
                kakuruBoard.Matrix[fillCellV.Row, fillCellV.Column] = fillCellV;


                //2.3 randomize the next point from one of the setted cells.
                if (rand.NextDouble() > 0.5)
                {
                    axis.X = fillCellH.Column;
                    axis.Y = fillCellH.Row;
                }
                else
                {
                    axis.X = fillCellV.Column;
                    axis.Y = fillCellV.Row;
                }

                //2.4 count all null cells
                nullCellCounter = 0;
                foreach (CellBase cell in kakuruBoard.Matrix)
                {
                    if (cell == null)
                    {
                        nullCellCounter++;
                    }
                }
            }
            // while subMatrix cells null count / total cells in subMatrix count is larger than factor(0.5)
            while (nullCellCounter > (int)((float)end.X * (float)end.Y * difficutlyRatio));

            //3. set all leftover null cells to be line cells
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (kakuruBoard.Matrix[i, j] == null)
                    {
                        CellKakuruLine lineCell = new CellKakuruLine();
                        lineCell.Row = i;
                        lineCell.Column = j;

                        kakuruBoard.Matrix[i, j] = lineCell;
                    }
                }
            }

            //4. return the new board
            return kakuruBoard;
        }

        private static BoardKakuru GenerateRandomStructuredMatrixLine(int rows, int columns, List<int> numbersRange)
        {
            BoardKakuru kakuruBoard = new BoardKakuru();

            kakuruBoard.NumberRange = numbersRange;

            ////Set random Structure
            Random rand = new Random();

            //create the matrix rows*columns
            kakuruBoard.Matrix = new CellBase[rows, columns];

            //set first row and column to line cells
            for (int i = 0; i < rows; i++)
            {
                CellKakuruLine lineCell = new CellKakuruLine();
                lineCell.Row = i;
                lineCell.Column = 0;
                kakuruBoard.Matrix[i, 0] = lineCell;
            }
            for (int j = 0; j < columns; j++)
            {
                CellKakuruLine lineCell = new CellKakuruLine();
                lineCell.Row = 0;
                lineCell.Column = j;
                kakuruBoard.Matrix[0, j] = lineCell;
            }

            //1. randomize a point in the subMatrix as a start point
            Point start = new Point(1, 1);
            Point end = new Point(rows - 1, columns - 1);

            Point axis = new Point(rand.Next(start.X, end.X), rand.Next(start.Y, end.Y));
            int nullCellCounter = 0;
            float difficutlyRatio = 0.5f;

            //2. do:
            do
            {
                bool rightLeft; // false -> right
                bool downUp; // false -> down
                //2.1 if point is at the edges of the subMatrix then select up or down and left or right accordingly
                if (axis.X == start.X)
                    rightLeft = false; //choose right
                else if (axis.X == end.X)
                    rightLeft = true; //choose left
                else
                    rightLeft = rand.NextDouble() > 0.5 ? false : true;//randomize left right choice


                if (axis.Y == start.Y)
                    downUp = false; //choose down
                else if (axis.Y == end.Y)
                    downUp = true; //choose up
                else
                    downUp = rand.NextDouble() > 0.5 ? false : true;//randomize down right up

                Point next = new Point();
                if (rand.NextDouble() > 0.5)
                {
                    //H
                    if (axis.X + (rightLeft ? -1 : 1) != next.X)
                    {
                        next.Y = axis.Y;
                        next.X = axis.X + (rightLeft ? -1 : 1);
                    }
                }
                else
                {
                    //V
                    next.Y = axis.Y + (downUp ? -1 : 1);
                    next.X = axis.X;
                }


                CellKakuruFill fillCellA = new CellKakuruFill();
                fillCellA.Column = axis.X;
                fillCellA.Row = axis.Y;

                CellKakuruFill fillCellN = new CellKakuruFill();
                fillCellN.Column = next.X;
                fillCellN.Row = next.Y;

                kakuruBoard.Matrix[fillCellA.Row, fillCellA.Column] = fillCellA;
                kakuruBoard.Matrix[fillCellN.Row, fillCellN.Column] = fillCellN;


                //2.3 randomize the next point from one of the setted cells.
                axis.X = fillCellN.Column;
                axis.Y = fillCellN.Row;

                //2.4 count all null cells
                nullCellCounter = 0;
                foreach (CellBase cell in kakuruBoard.Matrix)
                {
                    if (cell == null)
                    {
                        nullCellCounter++;
                    }
                }
            }
            // while subMatrix cells null count / total cells in subMatrix count is larger than factor(0.5)
            while (nullCellCounter > (int)((float)end.X * (float)end.Y * difficutlyRatio));

            //3. set all leftover null cells to be line cells
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (kakuruBoard.Matrix[i, j] == null)
                    {
                        CellKakuruLine lineCell = new CellKakuruLine();
                        lineCell.Row = i;
                        lineCell.Column = j;

                        kakuruBoard.Matrix[i, j] = lineCell;
                    }
                }
            }

            //4. return the new board
            return kakuruBoard;
        }
        */

        private static BoardKakuru GenerateRandomStructuredMatrixPlusSignLogicSymmetrical(int rows, int columns)
        {
            BoardKakuru kakuruBoard = new BoardKakuru();

            int maxGroupSizePossible = Math.Max(rows - 1, columns - 1);

            List<int> nums = new List<int>();
            for (int i = 0; i < maxGroupSizePossible; i++) nums.Add(i + 1);

            kakuruBoard.NumberRange = nums;

            ////Set random Structure
            Random rand = new Random();

            //create the matrix rows*columns
            CellBase[,] tempMatrix = new CellBase[rows + 3, columns + 3];

            //set first 2 rows and first 2 columns to line cells
            for (int i = 0; i < tempMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < tempMatrix.GetLength(1); j++)
                {
                    if (i < 2 || i > tempMatrix.GetLength(0) - 3 || j < 2 || j > tempMatrix.GetLength(1) - 3)
                    {
                        tempMatrix[i, j] = new CellGroupHolderKakuru(i, j);
                    }
                    else
                    {
                        tempMatrix[i, j] = new CellValueKakuru(i, j);
                    }
                }
            }
            

            //create a list of all the cells that left and need to be decided
            List<Point> cellsI = new List<Point>();

            for (int i = 0; i < tempMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < tempMatrix.GetLength(1); j++)
                {
                    if (tempMatrix[i, j].GetType() == typeof(CellValueKakuru))
                    {
                        cellsI.Add(new Point(i, j));
                    }
                }
            }


            
            while (cellsI.Count > 0)
            {
                Point randIndex = cellsI[rand.Next(0, cellsI.Count - 1)];

                //check logic rule for all directions from the random index position
                if (    tempMatrix[randIndex.X - 1, randIndex.Y].GetType() == typeof(CellValueKakuru) && tempMatrix[randIndex.X - 2, randIndex.Y].GetType() != typeof(CellValueKakuru)  //up is bad
                   ||   tempMatrix[randIndex.X + 1, randIndex.Y].GetType() == typeof(CellValueKakuru) && tempMatrix[randIndex.X + 2, randIndex.Y].GetType() != typeof(CellValueKakuru)  //down is bad
                   ||   tempMatrix[randIndex.X, randIndex.Y - 1].GetType() == typeof(CellValueKakuru) && tempMatrix[randIndex.X, randIndex.Y - 2].GetType() != typeof(CellValueKakuru) //left is bad 
                   ||   tempMatrix[randIndex.X, randIndex.Y + 1].GetType() == typeof(CellValueKakuru) && tempMatrix[randIndex.X, randIndex.Y + 2].GetType() != typeof(CellValueKakuru)) //right is bad
                {
                    //do nothing since the index will be removed anyway
                }
                else
                {
                    //all sides are ok to put a line cell
                    //put a line cell in the random index
                    tempMatrix[randIndex.X, randIndex.Y] = new CellGroupHolderKakuru(randIndex.X ,randIndex.Y);

                }

               for (int i = 0; i < cellsI.Count; i++)
                {
                    if (cellsI[i].X == randIndex.X && cellsI[i].Y == randIndex.Y)
                    {
                        cellsI.Remove(cellsI[i]);
                    }
                }

                //check if the new insert is splitting the fill cells to separated groups
                if (cellsI.Count > 0)
                {
                    Point randStartCellIndex = cellsI[rand.Next(0, cellsI.Count - 1)];
                    List<CellBase> PassedCells = new List<CellBase>();
                    DFSMatrixRecursive(tempMatrix, randStartCellIndex.X, randStartCellIndex.Y, PassedCells);

                    int fillCount = 0;
                    foreach (CellBase cell in tempMatrix)
                    {
                        if (cell.GetType() == typeof(CellValueKakuru))
                        {
                            fillCount++;
                        }
                    }

                    if (PassedCells.Count != fillCount)
                    {
                        //revert changes
                        //put back a fill cell in the random index
                        tempMatrix[randIndex.X, randIndex.Y] = new CellValueKakuru(randIndex.X, randIndex.Y);

                        //tempMatrix[tempMatrix.GetLength(0) - 1 - randIndex.X, tempMatrix.GetLength(1) - 1 - randIndex.Y] = new FillCell(tempMatrix.GetLength(0) - 1 - randIndex.X, tempMatrix.GetLength(1) - 1 - randIndex.Y);


                    }
                }
                
            }


            //pass through the matrix to look for places that were left out as null
            foreach (CellBase cell in tempMatrix)
            {
                if (cell == null)
                {
                    throw new Exception();
                }
            }


            //copy cells from the temporary matrix into our board matrix, as a sub matrix.
            CellBase[,] finalMatrix = new CellBase[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    finalMatrix[i, j] = tempMatrix[i + 1, j + 1];
                    finalMatrix[i, j].Row -= 1;
                    finalMatrix[i, j].Column -= 1;
                }
            }

            kakuruBoard.CellsMatrix = finalMatrix;

            return kakuruBoard;
        }


        private static void LinkMatrixCells(BoardKakuru board)
        {
            ////link between line cells and fill cells and vice versa

            int rows = board.CellsMatrix.GetLength(0), columns = board.CellsMatrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    CellGroupHolderKakuru lineCell = board.CellsMatrix[i, j] as CellGroupHolderKakuru;

                    if (lineCell != null)
                    {
                        int currRow = i;
                        while (currRow < rows - 1 && board.CellsMatrix[currRow + 1, j].GetType() == typeof(CellValueKakuru))
                        {
                            CellValueKakuru fillCell = board.CellsMatrix[currRow + 1, j] as CellValueKakuru;
                            if (lineCell.DownGroup == null)
                            {
                                lineCell.DownGroup = new GroupKakuru();
                                lineCell.DownGroup.Cells = new List<CellValueKakuru>();
                                lineCell.DownGroup.RightDown = true;
                            }

                            lineCell.DownGroup.Cells.Add(fillCell);
                            fillCell.Groups.Add(lineCell.DownGroup);

                            currRow++;
                        }

                        int currColumn = j;
                        while (currColumn < columns - 1 && board.CellsMatrix[i, currColumn + 1].GetType() == typeof(CellValueKakuru))
                        {
                            CellValueKakuru fillCell = board.CellsMatrix[i, currColumn + 1] as CellValueKakuru;
                            if (lineCell.RightGroup == null)
                            {
                                lineCell.RightGroup = new GroupKakuru();
                                lineCell.RightGroup.Cells = new List<CellValueKakuru>();
                                lineCell.RightGroup.RightDown = false;
                            }

                            lineCell.RightGroup.Cells.Add(fillCell);
                            fillCell.Groups.Add(lineCell.RightGroup);

                            currColumn++;
                        }
                    }
                }
            }
            ////
        }


        private static void RandomlySetAllFillCellsValuesRandomPlacement(BoardKakuru board)
        {
            //randomizing values in all fill cells and updating the total sums of related down and right line cells
            Random rand = new Random();

            for (int i = 0; i < board.CellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < board.CellsMatrix.GetLength(1); j++)
                {
                    CellValueKakuru fillCell = board.CellsMatrix[i, j] as CellValueKakuru;

                    if (fillCell != null)
                    {
                        List<int> possibleNumbers = new List<int>(board.NumberRange);

                        foreach (GroupKakuru cellGroup in fillCell.Groups)
                        {
                            foreach (CellValueKakuru relatedCell in cellGroup.Cells)
                            {
                                if (!object.ReferenceEquals(fillCell, relatedCell))
                                    if (relatedCell.IsFixed)
                                        possibleNumbers.Remove((int)relatedCell.Value);
                            }
                        }

                        if (possibleNumbers.Count > 0)
                        {
                            int selected = possibleNumbers[rand.Next(0, possibleNumbers.Count - 1)];
                            fillCell.Value = selected;
                            foreach (GroupKakuru group in fillCell.Groups)
                                group.Sum += selected;
                        }
                        else { }
                        //return false; //not valid -> cannot be that a cell cannot have a ny possible number. this may be caused by a group of cells which is larger than the possible number array.
                    }

                }
            }
        }

        private static bool RandomlySetAllFillCellsValuesByDifficulty(BoardKakuru board, float difficulty)
        {
            Random rand = new Random();

            //clearing all fill cells and all groups
            foreach (CellValueKakuru fillCell in board.ValueCells)
            {
                fillCell.Value = null;
            }
            foreach (GroupKakuru group in board.Groups)
            {
                group.Sum = 0;
            }


            //randomizing values in all fill cells and updating the total sums of related down and right line cells
            for (int i = 0; i < board.CellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < board.CellsMatrix.GetLength(1); j++)
                {
                    CellValueKakuru fillCell = board.CellsMatrix[i, j] as CellValueKakuru;

                    if (fillCell != null)
                    {
                        List<int> possibleNumbers = new List<int>(board.NumberRange);

                        foreach (GroupKakuru cellGroup in fillCell.Groups)
                        {
                            foreach (CellValueKakuru relatedCell in cellGroup.Cells)
                            {
                                if (!object.ReferenceEquals(fillCell, relatedCell))
                                    if (relatedCell.IsFixed)
                                        possibleNumbers.Remove((int)relatedCell.Value);
                            }
                        }

                        if (possibleNumbers.Count > 0)
                        {
                            int numberLevel = 0;
                            while (rand.NextDouble() < difficulty && numberLevel < possibleNumbers.Count - 1)
                            {
                                numberLevel++;
                            }
                            possibleNumbers.Sort();
                            int selected = possibleNumbers[numberLevel];
                            fillCell.Value = selected;
                            foreach (GroupKakuru group in fillCell.Groups)
                                group.Sum += selected;
                        }
                        else
                        {
                            throw new Exception();
                            //not valid -> cannot be that a cell cannot have a ny possible number. this may be caused by a group of cells which is larger than the possible number array.
                        }
                    }

                }
            }

            //test that all cells got a value! some passes can cause a cell to have no valid value as an outcome of the specific random placment
            bool isPlacmentValid = true;
            for (int i = 0; i < board.CellsMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < board.CellsMatrix.GetLength(1); j++)
                {
                    CellValueKakuru fillCell = board.CellsMatrix[i, j] as CellValueKakuru;

                    if (fillCell != null && fillCell.Value == null)
                    {
                        isPlacmentValid = false;
                    }
                }
            }

            return isPlacmentValid;
        }


        private static void FixingAllMustCells2(BoardKakuru board)
        {
            ////fixing all cells that must be fixed in order for the board to have single solution

            Random rand = new Random();

            List<CellValueKakuru> mustBeFixedFillCells = new List<CellValueKakuru>();

            //cloning solved board matrix
            int?[,] clonedMatrixValues = new int?[board.CellsMatrix.GetLength(0), board.CellsMatrix.GetLength(1)];
            for (int i = 0; i < board.CellsMatrix.GetLength(0); i++)
                for (int j = 0; j < board.CellsMatrix.GetLength(1); j++)
                {
                    CellValueKakuru fillCell = board.CellsMatrix[i,j] as CellValueKakuru;
                    if (fillCell != null)
                        clonedMatrixValues[i, j] = fillCell.Value;
                }

            //clering all values from original board matrix
            foreach (CellBase cell in board.CellsMatrix)
            {
                CellValueKakuru fillCell = cell as CellValueKakuru;
                if (fillCell != null)
                    fillCell.Value = null;
            }


            //trying to solve until getting to a dead end

            SolverKakuru solver = new SolverKakuru();
            solver.Board = board;

            solver.SolveInitiation(); //initiate board variations
            //board.SolveInitiation(); 

            while (!solver.IsSolved()) //while board is not solved yet
            {
                //do a complete step while there's any progress
                while (solver.DoCompleteStep()) ;

                if (solver.IsSolved()) //if we were able to solve the board
                {
                    break;
                }
                else
                {
                    //look for a fill cell which has no value and randomly set its value from one of the variation of one of the groups it belongs
                    //take the first empty fill cell found
                    CellValueKakuru emptyFillCell = null;
                    foreach (CellBase cell in board.CellsMatrix)
                    {
                        CellValueKakuru fillCell = cell as CellValueKakuru;
                        if (fillCell != null && fillCell.IsFixed == false)
                        {
                            emptyFillCell = fillCell;
                            break;
                        }
                    }

                    if (emptyFillCell != null) //if there was actually any empty fill cell (board indeed not solved)
                    {
                        emptyFillCell.Value = clonedMatrixValues[emptyFillCell.Row, emptyFillCell.Column];
                        if (!mustBeFixedFillCells.Contains(emptyFillCell))
                            mustBeFixedFillCells.Add(emptyFillCell);
                        else
                            throw new Exception();
                    }
                    else
                    {
                        //error -> no fill cells
                    }
                }
            }

            for (int i = 0; i < mustBeFixedFillCells.Count; i++)
            {
                //clear all fill cells
                foreach (CellBase cell in board.CellsMatrix)
                {
                    CellValueKakuru fillCell = cell as CellValueKakuru;

                    if (fillCell != null)
                    {
                        fillCell.Value = null;
                    }
                }

                //fix all must cells except the tested cell
                for (int j = 0; j < mustBeFixedFillCells.Count; j++)
                    if (j != i)
                        (board.CellsMatrix[mustBeFixedFillCells[j].Row, mustBeFixedFillCells[j].Column] as CellValueKakuru).Value = clonedMatrixValues[mustBeFixedFillCells[j].Row,mustBeFixedFillCells[j].Column];

                //try to solve with that tested cell unfixed
                solver.SolveInitiation();

                while (solver.DoCompleteStep()) ;

                if (solver.IsSolved()) //if the removal of this cell still enabled us to solve the board then it is not a must fix cell
                {
                    mustBeFixedFillCells.RemoveAt(i);
                    i--;
                }
                
            }


            //finally, clearing all fill cells but the must be fixed cells

            foreach (CellBase cell in board.CellsMatrix)
            {
                CellValueKakuru fillCell = cell as CellValueKakuru;

                if (fillCell != null)
                {
                    if (!mustBeFixedFillCells.Contains(fillCell))
                        fillCell.Value = null;
                    else
                        fillCell.Value = clonedMatrixValues[fillCell.Row, fillCell.Column];
                }
            }

            ////
        }



        public static void DFSMatrixRecursive(CellBase[,] matrix, int currRow, int currCol, List<CellBase> passedCells)
        {
            if (matrix[currRow, currCol].GetType() != typeof(CellValueKakuru))
            {
                return;
            }
            else
            {

                passedCells.Add(matrix[currRow, currCol]);

                //left
                if (currCol > 0)
                    if (!passedCells.Contains(matrix[currRow, currCol - 1]))
                        DFSMatrixRecursive(matrix, currRow, currCol - 1, passedCells);
                //right
                if (currCol < matrix.GetLength(1) - 1)
                    if (!passedCells.Contains(matrix[currRow, currCol + 1]))
                        DFSMatrixRecursive(matrix, currRow, currCol + 1, passedCells);
                //up
                if (currRow > 0)
                    if (!passedCells.Contains(matrix[currRow - 1, currCol]))
                        DFSMatrixRecursive(matrix, currRow - 1, currCol, passedCells);
                //down
                if (currRow < matrix.GetLength(0) - 1)
                    if (!passedCells.Contains(matrix[currRow + 1, currCol]))
                        DFSMatrixRecursive(matrix, currRow + 1, currCol, passedCells);

            }
        }

    }
}
