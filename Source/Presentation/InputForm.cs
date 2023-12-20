using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Presentation
{
    public partial class InputForm : Form
    {
        private string _data;

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }


        public InputForm()
        {
            InitializeComponent();
        }


        private void btn_ok_Click(object sender, EventArgs e)
        {
            this.Data = rtb_input.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}