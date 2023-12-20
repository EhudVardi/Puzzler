using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Presentation
{
    public partial class frm_MainForm : Form
    {
        PresentationLogicBase presentationLogicObject;

        public frm_MainForm()
        {
            InitializeComponent();

            this.pnl_drawingCanvas.MouseWheel += pnl_drawingCanvas_MouseWheel;
        }

        private void rb_puzzleTypeSelector_CheckedChanged(object sender, EventArgs e)
        {
            SelectPuzzleType();
        }


        private void btn_loadFromFile_Click(object sender, EventArgs e)
        {
            OpenPuzzle();
        }

        private void btn_loadFromWeb_Click(object sender, EventArgs e)
        {
            presentationLogicObject.ReadFromWeb(null);
        }

        private void btn_loadFromText_Click(object sender, EventArgs e)
        {
            OpenInputWindow();
        }

        private void btn_generateRandom_Click(object sender, EventArgs e)
        {
            presentationLogicObject.GenerateRandom();
        }


        private void frm_MainForm_Load(object sender, EventArgs e)
        {
            rb_sudoku.Select();
            SelectPuzzleType();
            rb_showBoard.Select();
        } 
         
        private void frm_MainForm_Refresh(object sender, EventArgs e)
        {
            RefreshForm();
        }

        private void frm_MainForm_Resize(object sender, EventArgs e)
        {
            RefreshForm();
        }

        private void frm_MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            presentationLogicObject.InputKey(e);
        }


        private void pnl_drawingCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (presentationLogicObject != null)
                presentationLogicObject.Draw(e);
        }

        private void pnl_drawingCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            presentationLogicObject.InputMouse(e, pnl_drawingCanvas.Size);
        }

        private void pnl_drawingCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            presentationLogicObject.InputMouseDown(e, pnl_drawingCanvas.Size);
        }

        private void pnl_drawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            presentationLogicObject.InputMouseMove(e, pnl_drawingCanvas.Size);
        }

        private void pnl_drawingCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            presentationLogicObject.InputMouseUp(e, pnl_drawingCanvas.Size);
        }


        void pnl_drawingCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            presentationLogicObject.InputMouseWheel(e, pnl_drawingCanvas.Size);
        }
       
        private void OpenPuzzle()
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.InitialDirectory = System.IO.Path.GetFullPath(presentationLogicObject.GetPuzzleTypeDocumentsPath() + Common.Configuration.PuzzlesLibraryFolder);
            opf.RestoreDirectory = false;
            opf.Multiselect = false;
            opf.Filter = "xml files (*.xml)|*.xml";

            if (opf.ShowDialog() == DialogResult.OK)
            {
                presentationLogicObject.ReadFromFile(opf.FileName);
            }
        }

        private void OpenInputWindow()
        {
            InputForm inputForm = new InputForm();
            inputForm.StartPosition = FormStartPosition.CenterParent;
            bool ok = false;
            while(!ok)
            {
                DialogResult dr = inputForm.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    if (presentationLogicObject.ReadFromText(inputForm.Data))
                        ok = true;
                }
                else if (dr == DialogResult.Cancel)
                    ok = true;
            }
        }



        private void SelectPuzzleType()
        {
            if (rb_sudoku.Checked)
                presentationLogicObject = new PresentationLogicSudoku();
            else if (rb_kakuru.Checked)
                presentationLogicObject = new PresentationLogicKakuru();
            else if (rb_griddler.Checked)
                presentationLogicObject = new PresentationLogicGriddler();
            else if (rb_griddlerRail.Checked)
                presentationLogicObject = new PresentationLogicGriddlerRails();

            if (presentationLogicObject != null)
            {
                presentationLogicObject.Initialize();
                presentationLogicObject.Refresh += new EventHandler(frm_MainForm_Refresh);
            }


            RefreshForm();
        }



        private void RefreshForm()
        {
            if (presentationLogicObject != null)
            {
                this.SuspendLayout();
                this.pnl_drawingCanvas.Refresh();
                this.tssl_boardStatusText.Text = string.Format("Valid: {0}, Solved: {1}", 
                    presentationLogicObject.IsValid(), 
                    presentationLogicObject.IsSolved());
                this.ResumeLayout();
            }
        }




        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }


        private void rb_show_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_showBoard.Checked)
                presentationLogicObject.ShowBoard();
            else if (rb_showHints.Checked)
                presentationLogicObject.ShowHints();
            else if (rb_showSolution.Checked)
                presentationLogicObject.ShowSolution();
            else { }

            RefreshForm();
        }




        
        
    }
}