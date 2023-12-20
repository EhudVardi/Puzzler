namespace Presentation
{
    partial class InputForm
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
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.rtb_input = new System.Windows.Forms.RichTextBox();
            this.tlp_main = new System.Windows.Forms.TableLayoutPanel();
            this.tlp_control = new System.Windows.Forms.TableLayoutPanel();
            this.tlp_main.SuspendLayout();
            this.tlp_control.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_ok
            // 
            this.btn_ok.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_ok.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_ok.Location = new System.Drawing.Point(1, 1);
            this.btn_ok.Margin = new System.Windows.Forms.Padding(1);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(78, 35);
            this.btn_ok.TabIndex = 0;
            this.btn_ok.Text = "OK";
            this.btn_ok.UseVisualStyleBackColor = true;
            this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_cancel.Location = new System.Drawing.Point(131, 1);
            this.btn_cancel.Margin = new System.Windows.Forms.Padding(1);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(78, 35);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // rtb_input
            // 
            this.rtb_input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_input.Location = new System.Drawing.Point(3, 3);
            this.rtb_input.Name = "rtb_input";
            this.rtb_input.Size = new System.Drawing.Size(210, 383);
            this.rtb_input.TabIndex = 2;
            this.rtb_input.Text = "";
            // 
            // tlp_main
            // 
            this.tlp_main.ColumnCount = 1;
            this.tlp_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_main.Controls.Add(this.tlp_control, 0, 1);
            this.tlp_main.Controls.Add(this.rtb_input, 0, 0);
            this.tlp_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlp_main.Location = new System.Drawing.Point(0, 0);
            this.tlp_main.Name = "tlp_main";
            this.tlp_main.RowCount = 2;
            this.tlp_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tlp_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlp_main.Size = new System.Drawing.Size(216, 432);
            this.tlp_main.TabIndex = 3;
            // 
            // tlp_control
            // 
            this.tlp_control.ColumnCount = 3;
            this.tlp_control.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlp_control.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_control.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlp_control.Controls.Add(this.btn_ok, 0, 0);
            this.tlp_control.Controls.Add(this.btn_cancel, 2, 0);
            this.tlp_control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlp_control.Location = new System.Drawing.Point(3, 392);
            this.tlp_control.Name = "tlp_control";
            this.tlp_control.RowCount = 1;
            this.tlp_control.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlp_control.Size = new System.Drawing.Size(210, 37);
            this.tlp_control.TabIndex = 2;
            // 
            // InputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(216, 432);
            this.Controls.Add(this.tlp_main);
            this.Name = "InputForm";
            this.Text = "InputForm";
            this.tlp_main.ResumeLayout(false);
            this.tlp_control.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.RichTextBox rtb_input;
        private System.Windows.Forms.TableLayoutPanel tlp_main;
        private System.Windows.Forms.TableLayoutPanel tlp_control;
    }
}