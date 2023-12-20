using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Base
{
    public abstract class BoardGenericBase<C, G, VC, GHC>
        where C : class
        where G : class
        where VC : class
        where GHC : class
    {

        public virtual int Size { get { return this.CellsMatrix.GetLength(0) * this.CellsMatrix.GetLength(1); } }


        protected C[,] _cellsMatrix;
        public C[,] CellsMatrix
        {
            get { return _cellsMatrix; }
            set { _cellsMatrix = value; }
        }

        protected List<G> _groups;
        public List<G> Groups
        {
            get { return _groups; }
            set { _groups = value; }
        }


        protected List<VC> _initialCells;
        public List<VC> InitialCells
        {
            get { return _initialCells; }
            set { _initialCells = value; }
        }


        public virtual List<VC> ValueCells 
        {
            get
            {
                List<VC> valueCells = new List<VC>();
                foreach (C cell in this.CellsMatrix)
                    if (cell.GetType() == typeof(VC))
                        valueCells.Add(cell as VC);
                return valueCells;
            }
        }

        public virtual List<GHC> GroupHolderCells 
        {
            get
            {
                List<GHC> valueCells = new List<GHC>();
                foreach (C cell in this.CellsMatrix)
                    if (cell.GetType() == typeof(GHC))
                        valueCells.Add(cell as GHC);
                return valueCells;
            }
        }


        public BoardGenericBase()
        {
            this.Groups = new List<G>();
            this.InitialCells = new List<VC>();
        }


        public virtual void SetCell(int row, int column, int num) { }



        public int Rows { get { return CellsMatrix.GetLength(0); } }

        public int Columns { get { return CellsMatrix.GetLength(1); } }

    }
     
}
