using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Presentation.WPF
{
    public sealed record PuzzleRow(
        string Type,
        string Name,
        string Source,
        string Size,
        DateTime DateCreated,
        string Path);

    public partial class ucPuzzlerDataGrid : UserControl
    {
        private DataTable _PuzzleDataTable = null!;
        public DataTable PuzzleDataTable { get { return _PuzzleDataTable; } }

        public class RequestLoadPuzzleEventArgs
        {
            public string Path { get; set; } = null!;
            public RequestLoadPuzzleEventArgs() { }
            public RequestLoadPuzzleEventArgs(string path) { this.Path = path; }
        }
        public delegate void RequestLoadPuzzleEventHandler(object sender, RequestLoadPuzzleEventArgs e);
        public event RequestLoadPuzzleEventHandler? RequestLoadPuzzle;
        protected virtual void OnRequestLoadPuzzle(RequestLoadPuzzleEventArgs e)
        {
            if (RequestLoadPuzzle != null)
                RequestLoadPuzzle(this, e);
        }

        public ucPuzzlerDataGrid()
        {
            InitializeComponent();
        }

        public void SetData(IReadOnlyList<PuzzleRow> rows)
        {
            _PuzzleDataTable = new DataTable();
            _PuzzleDataTable.Columns.Add(new DataColumn("Type",        typeof(string)));
            _PuzzleDataTable.Columns.Add(new DataColumn("Name",        typeof(string)));
            _PuzzleDataTable.Columns.Add(new DataColumn("Source",      typeof(string)));
            _PuzzleDataTable.Columns.Add(new DataColumn("Size",        typeof(string)));
            _PuzzleDataTable.Columns.Add(new DataColumn("DateCreated", typeof(DateTime)));
            _PuzzleDataTable.Columns.Add(new DataColumn("Path",        typeof(string)));

            foreach (var r in rows)
            {
                var row = _PuzzleDataTable.NewRow();
                row["Type"]        = r.Type        ?? "";
                row["Name"]        = r.Name        ?? "";
                row["Source"]      = r.Source      ?? "";
                row["Size"]        = r.Size        ?? "";
                row["DateCreated"] = r.DateCreated;
                row["Path"]        = System.IO.Path.GetFullPath(r.Path);
                _PuzzleDataTable.Rows.Add(row);
            }

            Binding b = new Binding();
            b.ElementName = "PuzzlerDataGrid";
            b.Path        = new PropertyPath("PuzzleDataTable");
            datagrid.SetBinding(DataGrid.ItemsSourceProperty, b);
        }

        private void datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadSelectedRowPuzzle();
        }
        private void datagrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    LoadSelectedRowPuzzle();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void LoadSelectedRowPuzzle()
        {
            if (datagrid.SelectedIndex == -1)
                return;

            DataRowView? drv = (datagrid.Items[datagrid.SelectedIndex] as DataRowView);
            if (drv == null)
                return;

            OnRequestLoadPuzzle(new RequestLoadPuzzleEventArgs((string)drv["Path"]));
        }
    }
}
