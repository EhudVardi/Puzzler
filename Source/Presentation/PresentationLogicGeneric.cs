using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Logic;
using System.Drawing;
using Data.DataModels;
using Data;
using System.IO;
using Common;
using Common.Models.Base;

namespace Presentation
{


    public class PresentationLogicGeneric<P,B> : PresentationLogicBase
    {

        public LogicLayerGeneric<P, B> LogicProxy;

        public string URL;

        
        public PresentationLogicGeneric()
        {

        }

        public override void Initialize()
        {
            this.LogicProxy.SolveCompleted += new EventHandler(Board_SolveCompleted);
            this.LogicProxy.StepCompleted += new EventHandler(Board_StepCompleted);
            this.LogicProxy.LoadCompleted += new EventHandler(LogicProxy_LoadCompleted);
            this.LogicProxy.StepGenerated += new EventHandler(LogicProxy_StepGenerated);
        }

        
        public override bool ReadFromFile(string fileName) {return this.LogicProxy.ReadFromFile(fileName);}

        public override bool ReadFromWeb(string url)  { return this.LogicProxy.ReadFromWeb(this.URL); }

        public override bool ReadFromText(string text) { return LogicProxy.ReadFromText(text); }


        public override bool GenerateRandom()  { return LogicProxy.GenerateRandom(); }

        
        public override string GetPuzzleTypeDocumentsPath() { return LogicProxy.GetPuzzleTypeDocumentsPath(); }

        public virtual string GetPuzzleName() { return LogicProxy.GetPuzzleName(); }



        protected void Board_StepCompleted(object sender, EventArgs e) { this.OnRequestRefresh(EventArgs.Empty); }

        protected void Board_SolveCompleted(object sender, EventArgs e) { this.OnRequestRefresh(EventArgs.Empty); }


        protected void LogicProxy_LoadCompleted(object sender, EventArgs e)
        {
            this.InitDisplay();

            this.OnRequestRefresh(EventArgs.Empty);
        }

        protected void LogicProxy_StepGenerated(object sender, EventArgs e)
        {
            this.OnRequestRefresh(EventArgs.Empty);
        }


        public override bool? IsSolved()  { return LogicProxy.RequestSolveStatus(); }

        public override bool? IsValid()  { return LogicProxy.RequestValidStatus(); }


        public override void Draw(PaintEventArgs e)
        {
            using (Font font2 = new Font("Arial", Math.Max(Math.Min(e.ClipRectangle.Width, e.ClipRectangle.Height), 1) / (float)this.GetPuzzleName().Length, FontStyle.Italic, GraphicsUnit.Pixel))
            {
                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                TextRenderer.DrawText(e.Graphics, this.GetPuzzleName(), font2, e.ClipRectangle, Color.Black, flags);
                e.Graphics.DrawRectangle(Pens.Black, Rectangle.Round(e.ClipRectangle));
            }
        }


        public virtual B GetTrackerBoard() { return this.LogicProxy.getSolvedBoard(); }

        public virtual B GetSolvedBoard() { return this.LogicProxy.getTrackedBoard(); }

    }
}
