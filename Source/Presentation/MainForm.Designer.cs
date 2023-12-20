namespace Presentation
{
    partial class frm_MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlp_MainLayoutTable = new System.Windows.Forms.TableLayoutPanel();
            this.tlp_ControlLayoutTable = new System.Windows.Forms.TableLayoutPanel();
            this.gb_puzzleSelector = new System.Windows.Forms.GroupBox();
            this.rb_griddlerRail = new System.Windows.Forms.RadioButton();
            this.rb_griddler = new System.Windows.Forms.RadioButton();
            this.rb_kakuru = new System.Windows.Forms.RadioButton();
            this.rb_sudoku = new System.Windows.Forms.RadioButton();
            this.gb_load = new System.Windows.Forms.GroupBox();
            this.btn_loadFromText = new System.Windows.Forms.Button();
            this.btn_loadFromWeb = new System.Windows.Forms.Button();
            this.btn_loadFromFile = new System.Windows.Forms.Button();
            this.gb_generate = new System.Windows.Forms.GroupBox();
            this.btn_generateRandom = new System.Windows.Forms.Button();
            this.gb_solveControl = new System.Windows.Forms.GroupBox();
            this.rb_showSolution = new System.Windows.Forms.RadioButton();
            this.rb_showHints = new System.Windows.Forms.RadioButton();
            this.rb_showBoard = new System.Windows.Forms.RadioButton();
            this.pnl_drawingCanvas = new System.Windows.Forms.Panel();
            this.sts_statusStrip = new System.Windows.Forms.StatusStrip();
            this.tssl_boardStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssl_boardStatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.tlp_MainLayoutTable.SuspendLayout();
            this.tlp_ControlLayoutTable.SuspendLayout();
            this.gb_puzzleSelector.SuspendLayout();
            this.gb_load.SuspendLayout();
            this.gb_generate.SuspendLayout();
            this.gb_solveControl.SuspendLayout();
            this.sts_statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlp_MainLayoutTable
            // 
            this.tlp_MainLayoutTable.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tlp_MainLayoutTable.ColumnCount = 2;
            this.tlp_MainLayoutTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 114F));
            this.tlp_MainLayoutTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_MainLayoutTable.Controls.Add(this.tlp_ControlLayoutTable, 0, 0);
            this.tlp_MainLayoutTable.Controls.Add(this.pnl_drawingCanvas, 1, 0);
            this.tlp_MainLayoutTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlp_MainLayoutTable.Location = new System.Drawing.Point(0, 0);
            this.tlp_MainLayoutTable.Name = "tlp_MainLayoutTable";
            this.tlp_MainLayoutTable.RowCount = 1;
            this.tlp_MainLayoutTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_MainLayoutTable.Size = new System.Drawing.Size(663, 518);
            this.tlp_MainLayoutTable.TabIndex = 0;
            // 
            // tlp_ControlLayoutTable
            // 
            this.tlp_ControlLayoutTable.ColumnCount = 1;
            this.tlp_ControlLayoutTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_ControlLayoutTable.Controls.Add(this.gb_puzzleSelector, 0, 0);
            this.tlp_ControlLayoutTable.Controls.Add(this.gb_load, 0, 1);
            this.tlp_ControlLayoutTable.Controls.Add(this.gb_generate, 0, 2);
            this.tlp_ControlLayoutTable.Controls.Add(this.gb_solveControl, 0, 3);
            this.tlp_ControlLayoutTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlp_ControlLayoutTable.Location = new System.Drawing.Point(4, 4);
            this.tlp_ControlLayoutTable.Name = "tlp_ControlLayoutTable";
            this.tlp_ControlLayoutTable.RowCount = 5;
            this.tlp_ControlLayoutTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.tlp_ControlLayoutTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tlp_ControlLayoutTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tlp_ControlLayoutTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tlp_ControlLayoutTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_ControlLayoutTable.Size = new System.Drawing.Size(108, 510);
            this.tlp_ControlLayoutTable.TabIndex = 0;
            // 
            // gb_puzzleSelector
            // 
            this.gb_puzzleSelector.Controls.Add(this.rb_griddlerRail);
            this.gb_puzzleSelector.Controls.Add(this.rb_griddler);
            this.gb_puzzleSelector.Controls.Add(this.rb_kakuru);
            this.gb_puzzleSelector.Controls.Add(this.rb_sudoku);
            this.gb_puzzleSelector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gb_puzzleSelector.Location = new System.Drawing.Point(3, 3);
            this.gb_puzzleSelector.Name = "gb_puzzleSelector";
            this.gb_puzzleSelector.Size = new System.Drawing.Size(102, 120);
            this.gb_puzzleSelector.TabIndex = 1;
            this.gb_puzzleSelector.TabStop = false;
            this.gb_puzzleSelector.Text = "Puzzle Type";
            // 
            // rb_griddlerRail
            // 
            this.rb_griddlerRail.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_griddlerRail.AutoSize = true;
            this.rb_griddlerRail.Dock = System.Windows.Forms.DockStyle.Top;
            this.rb_griddlerRail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rb_griddlerRail.Location = new System.Drawing.Point(3, 91);
            this.rb_griddlerRail.Name = "rb_griddlerRail";
            this.rb_griddlerRail.Size = new System.Drawing.Size(96, 25);
            this.rb_griddlerRail.TabIndex = 3;
            this.rb_griddlerRail.TabStop = true;
            this.rb_griddlerRail.Text = "Griddler Rail";
            this.rb_griddlerRail.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rb_griddlerRail.UseVisualStyleBackColor = true;
            this.rb_griddlerRail.CheckedChanged += new System.EventHandler(this.rb_puzzleTypeSelector_CheckedChanged);
            // 
            // rb_griddler
            // 
            this.rb_griddler.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_griddler.AutoSize = true;
            this.rb_griddler.Dock = System.Windows.Forms.DockStyle.Top;
            this.rb_griddler.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rb_griddler.Location = new System.Drawing.Point(3, 66);
            this.rb_griddler.Name = "rb_griddler";
            this.rb_griddler.Size = new System.Drawing.Size(96, 25);
            this.rb_griddler.TabIndex = 2;
            this.rb_griddler.TabStop = true;
            this.rb_griddler.Text = "Griddler";
            this.rb_griddler.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rb_griddler.UseVisualStyleBackColor = true;
            this.rb_griddler.CheckedChanged += new System.EventHandler(this.rb_puzzleTypeSelector_CheckedChanged);
            // 
            // rb_kakuru
            // 
            this.rb_kakuru.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_kakuru.AutoSize = true;
            this.rb_kakuru.Dock = System.Windows.Forms.DockStyle.Top;
            this.rb_kakuru.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rb_kakuru.Location = new System.Drawing.Point(3, 41);
            this.rb_kakuru.Name = "rb_kakuru";
            this.rb_kakuru.Size = new System.Drawing.Size(96, 25);
            this.rb_kakuru.TabIndex = 1;
            this.rb_kakuru.TabStop = true;
            this.rb_kakuru.Text = "Kakuru";
            this.rb_kakuru.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rb_kakuru.UseVisualStyleBackColor = true;
            this.rb_kakuru.CheckedChanged += new System.EventHandler(this.rb_puzzleTypeSelector_CheckedChanged);
            // 
            // rb_sudoku
            // 
            this.rb_sudoku.Appearance = System.Windows.Forms.Appearance.Button;
            this.rb_sudoku.AutoSize = true;
            this.rb_sudoku.Dock = System.Windows.Forms.DockStyle.Top;
            this.rb_sudoku.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rb_sudoku.Location = new System.Drawing.Point(3, 16);
            this.rb_sudoku.Name = "rb_sudoku";
            this.rb_sudoku.Size = new System.Drawing.Size(96, 25);
            this.rb_sudoku.TabIndex = 0;
            this.rb_sudoku.TabStop = true;
            this.rb_sudoku.Text = "Sudoku";
            this.rb_sudoku.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rb_sudoku.UseVisualStyleBackColor = true;
            this.rb_sudoku.CheckedChanged += new System.EventHandler(this.rb_puzzleTypeSelector_CheckedChanged);
            // 
            // gb_load
            // 
            this.gb_load.Controls.Add(this.btn_loadFromText);
            this.gb_load.Controls.Add(this.btn_loadFromWeb);
            this.gb_load.Controls.Add(this.btn_loadFromFile);
            this.gb_load.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gb_load.Location = new System.Drawing.Point(3, 129);
            this.gb_load.Name = "gb_load";
            this.gb_load.Size = new System.Drawing.Size(102, 90);
            this.gb_load.TabIndex = 2;
            this.gb_load.TabStop = false;
            this.gb_load.Text = "Load";
            // 
            // btn_loadFromText
            // 
            this.btn_loadFromText.Dock = System.Windows.Forms.DockStyle.Top;
            this.btn_loadFromText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_loadFromText.Location = new System.Drawing.Point(3, 62);
            this.btn_loadFromText.Name = "btn_loadFromText";
            this.btn_loadFromText.Size = new System.Drawing.Size(96, 23);
            this.btn_loadFromText.TabIndex = 2;
            this.btn_loadFromText.Text = "Text";
            this.btn_loadFromText.UseVisualStyleBackColor = true;
            this.btn_loadFromText.Click += new System.EventHandler(this.btn_loadFromText_Click);
            // 
            // btn_loadFromWeb
            // 
            this.btn_loadFromWeb.Dock = System.Windows.Forms.DockStyle.Top;
            this.btn_loadFromWeb.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_loadFromWeb.Location = new System.Drawing.Point(3, 39);
            this.btn_loadFromWeb.Name = "btn_loadFromWeb";
            this.btn_loadFromWeb.Size = new System.Drawing.Size(96, 23);
            this.btn_loadFromWeb.TabIndex = 1;
            this.btn_loadFromWeb.Text = "Web";
            this.btn_loadFromWeb.UseVisualStyleBackColor = true;
            this.btn_loadFromWeb.Click += new System.EventHandler(this.btn_loadFromWeb_Click);
            // 
            // btn_loadFromFile
            // 
            this.btn_loadFromFile.Dock = System.Windows.Forms.DockStyle.Top;
            this.btn_loadFromFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_loadFromFile.Location = new System.Drawing.Point(3, 16);
            this.btn_loadFromFile.Name = "btn_loadFromFile";
            this.btn_loadFromFile.Size = new System.Drawing.Size(96, 23);
            this.btn_loadFromFile.TabIndex = 0;
            this.btn_loadFromFile.Text = "File";
            this.btn_loadFromFile.UseVisualStyleBackColor = true;
            this.btn_loadFromFile.Click += new System.EventHandler(this.btn_loadFromFile_Click);
            // 
            // gb_generate
            // 
            this.gb_generate.Controls.Add(this.btn_generateRandom);
            this.gb_generate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gb_generate.Location = new System.Drawing.Point(3, 225);
            this.gb_generate.Name = "gb_generate";
            this.gb_generate.Size = new System.Drawing.Size(102, 42);
            this.gb_generate.TabIndex = 3;
            this.gb_generate.TabStop = false;
            this.gb_generate.Text = "Generate";
            // 
            // btn_generateRandom
            // 
            this.btn_generateRandom.Dock = System.Windows.Forms.DockStyle.Top;
            this.btn_generateRandom.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_generateRandom.Location = new System.Drawing.Point(3, 16);
            this.btn_generateRandom.Name = "btn_generateRandom";
            this.btn_generateRandom.Size = new System.Drawing.Size(96, 23);
            this.btn_generateRandom.TabIndex = 1;
            this.btn_generateRandom.Text = "Random";
            this.btn_generateRandom.UseVisualStyleBackColor = true;
            this.btn_generateRandom.Click += new System.EventHandler(this.btn_generateRandom_Click);
            // 
            // gb_solveControl
            // 
            this.gb_solveControl.Controls.Add(this.rb_showSolution);
            this.gb_solveControl.Controls.Add(this.rb_showHints);
            this.gb_solveControl.Controls.Add(this.rb_showBoard);
            this.gb_solveControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gb_solveControl.Location = new System.Drawing.Point(3, 273);
            this.gb_solveControl.Name = "gb_solveControl";
            this.gb_solveControl.Size = new System.Drawing.Size(102, 134);
            this.gb_solveControl.TabIndex = 4;
            this.gb_solveControl.TabStop = false;
            this.gb_solveControl.Text = "Solution Display";
            // 
            // rb_showSolution
            // 
            this.rb_showSolution.AutoSize = true;
            this.rb_showSolution.Dock = System.Windows.Forms.DockStyle.Top;
            this.rb_showSolution.Location = new System.Drawing.Point(3, 70);
            this.rb_showSolution.Name = "rb_showSolution";
            this.rb_showSolution.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.rb_showSolution.Size = new System.Drawing.Size(96, 27);
            this.rb_showSolution.TabIndex = 2;
            this.rb_showSolution.TabStop = true;
            this.rb_showSolution.Text = "Solved";
            this.rb_showSolution.UseVisualStyleBackColor = true;
            this.rb_showSolution.CheckedChanged += new System.EventHandler(this.rb_show_CheckedChanged);
            // 
            // rb_showHints
            // 
            this.rb_showHints.AutoSize = true;
            this.rb_showHints.Dock = System.Windows.Forms.DockStyle.Top;
            this.rb_showHints.Location = new System.Drawing.Point(3, 43);
            this.rb_showHints.Name = "rb_showHints";
            this.rb_showHints.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.rb_showHints.Size = new System.Drawing.Size(96, 27);
            this.rb_showHints.TabIndex = 1;
            this.rb_showHints.TabStop = true;
            this.rb_showHints.Text = "Hints";
            this.rb_showHints.UseVisualStyleBackColor = true;
            this.rb_showHints.CheckedChanged += new System.EventHandler(this.rb_show_CheckedChanged);
            // 
            // rb_showBoard
            // 
            this.rb_showBoard.AutoSize = true;
            this.rb_showBoard.Dock = System.Windows.Forms.DockStyle.Top;
            this.rb_showBoard.Location = new System.Drawing.Point(3, 16);
            this.rb_showBoard.Name = "rb_showBoard";
            this.rb_showBoard.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.rb_showBoard.Size = new System.Drawing.Size(96, 27);
            this.rb_showBoard.TabIndex = 0;
            this.rb_showBoard.TabStop = true;
            this.rb_showBoard.Text = "Clean";
            this.rb_showBoard.UseVisualStyleBackColor = true;
            this.rb_showBoard.CheckedChanged += new System.EventHandler(this.rb_show_CheckedChanged);
            // 
            // pnl_drawingCanvas
            // 
            this.pnl_drawingCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl_drawingCanvas.Location = new System.Drawing.Point(119, 4);
            this.pnl_drawingCanvas.Name = "pnl_drawingCanvas";
            this.pnl_drawingCanvas.Size = new System.Drawing.Size(540, 510);
            this.pnl_drawingCanvas.TabIndex = 1;
            this.pnl_drawingCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.pnl_drawingCanvas_Paint);
            this.pnl_drawingCanvas.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnl_drawingCanvas_MouseClick);
            this.pnl_drawingCanvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnl_drawingCanvas_MouseDown);
            this.pnl_drawingCanvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnl_drawingCanvas_MouseMove);
            this.pnl_drawingCanvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnl_drawingCanvas_MouseUp);
            // 
            // sts_statusStrip
            // 
            this.sts_statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssl_boardStatusLabel,
            this.tssl_boardStatusText});
            this.sts_statusStrip.Location = new System.Drawing.Point(0, 496);
            this.sts_statusStrip.Name = "sts_statusStrip";
            this.sts_statusStrip.Size = new System.Drawing.Size(663, 22);
            this.sts_statusStrip.TabIndex = 1;
            this.sts_statusStrip.Text = "statusStrip1";
            // 
            // tssl_boardStatusLabel
            // 
            this.tssl_boardStatusLabel.Name = "tssl_boardStatusLabel";
            this.tssl_boardStatusLabel.Size = new System.Drawing.Size(42, 17);
            this.tssl_boardStatusLabel.Text = "Status:";
            // 
            // tssl_boardStatusText
            // 
            this.tssl_boardStatusText.Name = "tssl_boardStatusText";
            this.tssl_boardStatusText.Size = new System.Drawing.Size(55, 17);
            this.tssl_boardStatusText.Text = "<Status>";
            // 
            // frm_MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(663, 518);
            this.Controls.Add(this.sts_statusStrip);
            this.Controls.Add(this.tlp_MainLayoutTable);
            this.KeyPreview = true;
            this.Name = "frm_MainForm";
            this.Text = "Solvers";
            this.Load += new System.EventHandler(this.frm_MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frm_MainForm_KeyDown);
            this.Resize += new System.EventHandler(this.frm_MainForm_Resize);
            this.tlp_MainLayoutTable.ResumeLayout(false);
            this.tlp_ControlLayoutTable.ResumeLayout(false);
            this.gb_puzzleSelector.ResumeLayout(false);
            this.gb_puzzleSelector.PerformLayout();
            this.gb_load.ResumeLayout(false);
            this.gb_generate.ResumeLayout(false);
            this.gb_solveControl.ResumeLayout(false);
            this.gb_solveControl.PerformLayout();
            this.sts_statusStrip.ResumeLayout(false);
            this.sts_statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlp_MainLayoutTable;
        private System.Windows.Forms.TableLayoutPanel tlp_ControlLayoutTable;
        private System.Windows.Forms.RadioButton rb_griddler;
        private System.Windows.Forms.RadioButton rb_kakuru;
        private System.Windows.Forms.RadioButton rb_sudoku;
        private System.Windows.Forms.GroupBox gb_puzzleSelector;
        private System.Windows.Forms.GroupBox gb_load;
        private System.Windows.Forms.Button btn_loadFromText;
        private System.Windows.Forms.Button btn_loadFromWeb;
        private System.Windows.Forms.Button btn_loadFromFile;
        private System.Windows.Forms.GroupBox gb_generate;
        private System.Windows.Forms.Button btn_generateRandom;
        private System.Windows.Forms.GroupBox gb_solveControl;
        private System.Windows.Forms.Panel pnl_drawingCanvas;
        private System.Windows.Forms.StatusStrip sts_statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tssl_boardStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel tssl_boardStatusText;
        private System.Windows.Forms.RadioButton rb_showSolution;
        private System.Windows.Forms.RadioButton rb_showHints;
        private System.Windows.Forms.RadioButton rb_showBoard;
        private System.Windows.Forms.RadioButton rb_griddlerRail;
    }
}