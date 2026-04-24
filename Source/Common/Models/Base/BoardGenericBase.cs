using System;
using System.Collections.Generic;

namespace Common.Models.Base
{
    public abstract class BoardGenericBase<TCell, TGroup, TValueCell, TGroupHolder>
        where TCell       : class
        where TGroup      : class
        where TValueCell  : class
        where TGroupHolder : class
    {
        public virtual int Size => CellsMatrix.GetLength(0) * CellsMatrix.GetLength(1);

        protected TCell[,] _cellsMatrix = null!;
        public TCell[,] CellsMatrix
        {
            get { return _cellsMatrix; }
            set { _cellsMatrix = value; }
        }

        protected List<TGroup> _groups = null!;
        public List<TGroup> Groups
        {
            get { return _groups; }
            set { _groups = value; }
        }

        protected List<TValueCell> _initialCells = null!;
        public List<TValueCell> InitialCells
        {
            get { return _initialCells; }
            set { _initialCells = value; }
        }

        public virtual List<TValueCell> ValueCells
        {
            get
            {
                List<TValueCell> valueCells = new List<TValueCell>();
                foreach (TCell cell in this.CellsMatrix)
                    if (cell.GetType() == typeof(TValueCell) && cell is TValueCell vc)
                        valueCells.Add(vc);
                return valueCells;
            }
        }

        public virtual List<TGroupHolder> GroupHolderCells
        {
            get
            {
                List<TGroupHolder> cells = new List<TGroupHolder>();
                foreach (TCell cell in this.CellsMatrix)
                    if (cell.GetType() == typeof(TGroupHolder) && cell is TGroupHolder gh)
                        cells.Add(gh);
                return cells;
            }
        }

        public BoardGenericBase()
        {
            this.Groups       = new List<TGroup>();
            this.InitialCells = new List<TValueCell>();
        }

        public virtual void SetCell(int row, int column, int num) { }

        public virtual TCell? GetCell(int row, int column) { return default(TCell); }

        public virtual int Rows    => CellsMatrix.GetLength(0);
        public virtual int Columns => CellsMatrix.GetLength(1);
    }
}
