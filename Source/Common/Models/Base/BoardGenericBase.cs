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
            set
            {
                _cellsMatrix = value;
                _valueCellsCache = null;
                _groupHolderCellsCache = null;
            }
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

        private List<TValueCell>? _valueCellsCache;
        public virtual List<TValueCell> ValueCells
        {
            get
            {
                if (_valueCellsCache == null)
                {
                    _valueCellsCache = new List<TValueCell>();
                    foreach (TCell cell in _cellsMatrix)
                        if (cell is TValueCell vc)
                            _valueCellsCache.Add(vc);
                }
                return _valueCellsCache;
            }
        }

        private List<TGroupHolder>? _groupHolderCellsCache;
        public virtual List<TGroupHolder> GroupHolderCells
        {
            get
            {
                if (_groupHolderCellsCache == null)
                {
                    _groupHolderCellsCache = new List<TGroupHolder>();
                    foreach (TCell cell in _cellsMatrix)
                        if (cell is TGroupHolder gh)
                            _groupHolderCellsCache.Add(gh);
                }
                return _groupHolderCellsCache;
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
