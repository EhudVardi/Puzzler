using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PresentationLogic;
using System.Drawing;
using System.IO;
using System.Data;

namespace Presentation.WPF
{
    public partial class ucPuzzlerDataGrid : UserControl
    {
        private DataTable _GameData;
        public DataTable GameData { get { return _GameData; } }

        public class RequestLoadPuzzleEventArgs
        {
            public string Path { get; set; }
            public RequestLoadPuzzleEventArgs() { }
            public RequestLoadPuzzleEventArgs(string path) { this.Path = path; }
        }
        public delegate void RequestLoadPuzzleEventHandler(object sender, RequestLoadPuzzleEventArgs e);
        public event RequestLoadPuzzleEventHandler RequestLoadPuzzle;
        protected virtual void OnRequestLoadPuzzle(RequestLoadPuzzleEventArgs e)
        {
            if (RequestLoadPuzzle != null)
                RequestLoadPuzzle(this, e);
        }

        public ucPuzzlerDataGrid()
        {
            InitializeComponent();
        }

        public void SetData(List<string> data)
        {
            _GameData = new DataTable();
            _GameData.Columns.Add(new DataColumn("Puzzle Name", typeof(string)));
            _GameData.Columns.Add(new DataColumn("Path", typeof(string)));

            foreach (string file in data)
            {
                var row = _GameData.NewRow();
                _GameData.Rows.Add(row);

                row["Puzzle Name"] = System.IO.Path.GetFileName(file);
                row["Path"] = System.IO.Path.GetFullPath(file);
            }
            
            //ItemsSource="{Binding ElementName=This, Path=GameData}"
            Binding b = new Binding();
            b.ElementName = "PuzzlerDataGrid";
            b.Path = new PropertyPath("GameData");
            datagrid.SetBinding(DataGrid.ItemsSourceProperty, b);
            //
        }

        private void datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (datagrid.SelectedIndex == -1)
                return; 

            DataRowView drv = (datagrid.Items[datagrid.SelectedIndex] as DataRowView);
            if (drv == null)
                return;

            OnRequestLoadPuzzle(new RequestLoadPuzzleEventArgs((string)drv[1]));
        }
    }
}
