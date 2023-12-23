using System;
using System.Collections.Generic;
using System.Text;
using Common;
using Data;
using Data.DataModels;
using Common.Models.Triddler;
using Logic.Griddler;

namespace Logic
{
    public class LogicLayerTriddler : LogicLayerGeneric<PuzzleTriddler, BoardTriddler>
    {
        public LogicLayerTriddler()
        {



            this.DataProxy = new DataLayerTriddler();

            this.FactoryModule = new FactoryTriddler();
            this.SolverModule = new SolverTriddler();
            AttachSolverEvents();






            //////TESTING CODE
            //List<int> H1 = new List<int>(new int[] { 2 });
            //List<int> H2 = new List<int>(new int[] { 6 });
            //List<int> H3 = new List<int>(new int[] { 2 });
            //List<List<int>> Hs = new List<List<int>>();
            //Hs.Add(H1);
            //Hs.Add(H2);
            //Hs.Add(H3);
            //List<int> V1 = new List<int>(new int[] { 2 });
            //List<int> V2 = new List<int>(new int[] { 6 });
            //List<int> V3 = new List<int>(new int[] { 2 });
            //List<List<int>> Vs = new List<List<int>>();
            //Vs.Add(V1);
            //Vs.Add(V2);
            //Vs.Add(V3);
            //List<int> D1 = new List<int>(new int[] { });
            //List<int> D2 = new List<int>(new int[] { 1, 1 });
            //List<int> D3 = new List<int>(new int[] { 3 });
            //List<int> D4 = new List<int>(new int[] { 3 });
            //List<int> D5 = new List<int>(new int[] { 1, 1 });
            //List<int> D6 = new List<int>(new int[] { });
            //List<List<int>> Ds = new List<List<int>>();
            //Ds.Add(D1);
            //Ds.Add(D2);
            //Ds.Add(D3);
            //Ds.Add(D4);
            //Ds.Add(D5);
            //Ds.Add(D6);
            //Data.DataModels.PuzzleTriddler pt = new PuzzleTriddler(Hs, Vs, Ds, 3, 3, 3, 0, 3, 0);
            //
            //DataProxy.WritePuzzle(pt, Configuration.FromTextFolder);
            //FactoryModule.PuzzleToBoard(pt);
            //////
        }
    }
}
