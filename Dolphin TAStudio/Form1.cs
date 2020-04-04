/* TODO: 
- inputView_MouseClick: Implement the user right clicking and selecting multiple rows
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Dolphin_TAStudio
{
    public partial class Form1 : Form
    {
        private DataTable table = new DataTable();
        private string fileName;
        private bool changed = false;

        // Represent columns in a list of tuples, to allow for easy modification should these column names change
        private List<(string, string)> columnNames = new List<(string, string)>()
        {
            ("Save?", "Bool"),
            ("Frame", "Int"),
            ("aX", "Int"),
            ("aY", "Int"),
            ("A", "Bool"),
            ("B", "Bool"),
            ("X", "Bool"),
            ("Y", "Bool"),
            ("S", "Bool"),
            ("Z", "Bool"),
            ("L", "Bool"),
            ("R", "Bool"),
            ("La", "Int"),
            ("Ra", "Int"),
            ("dU", "Bool"),
            ("dD", "Bool"),
            ("dL", "Bool"),
            ("dR", "Bool"),
            ("cX", "Int"),
            ("cY", "Int")
        };

        public Form1()
        {
            InitializeComponent();
            disableMenuButtons();
        }

        private void disableMenuButtons()
        {
            copyToolStripMenuItem.Enabled = false;
            pasteToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
        }

        private void openData(string fileLocation)
        {
            // Read from file and assert that it is formatted properly
            using (StreamReader reader = new StreamReader(fileLocation))
            {
                string data = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    // TODO: Implement parser
                }
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement save before closing
            // if (changed)....

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                clearDataTable();
                fileName = openFileDialog1.FileName;
                openData(fileName);
            }
        }

        private void clearDataTable()
        {
            table.Columns.Clear();
            table.Clear();
            undoToolStripMenuItem.Enabled = false;
            fileName = null;
            changed = false;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void pasteCtrlVToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void tableGenerateColumns()
        {
            foreach ((string, string) column in columnNames)
            {
                if (column.Item2 == "Int") table.Columns.Add(column.Item1, typeof(Int16));
                else if (column.Item2 == "Bool") table.Columns.Add(column.Item1, typeof(bool));
            }
        }

        private DataRow tableGenerateDefaultRow()
        {
            DataRow newRow = table.NewRow();
            for (int i = 1; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == "Frame")
                {
                    newRow[i] = table.Rows.Count + 1;
                }
                else if (table.Columns[i].DataType == System.Type.GetType("System.Int16")) newRow[i] = 128;
            }

            return newRow;
        }

        private void resizeInputViewColumns()
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                int columnNameLength = table.Columns[i].ColumnName.Length;
                // Scale column width based on the length of the column name
                inputView.Columns[i].Width = (5 * columnNameLength) + 20;


                if (table.Columns[i].ColumnName == "Frame")
                {
                    inputView.Columns[i].ReadOnly = true;
                }
            }
        }

        private void newCtrlNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Save before quitting an open table

            // Generate a blank table with one default row
            tableGenerateColumns();
            table.Rows.Add(tableGenerateDefaultRow());
            inputView.DataSource = table;
            resizeInputViewColumns();
        }

        private void inputView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // TODO: Implement the user right clicking and selecting multiple rows

                // If the user right clicks while a row(s) is/are selected, copy the row(s)
                
                // If multiple cells are selected, then highlight the entire row of all cells
                foreach (DataGridViewCell cell in inputView.SelectedCells)
                {
                    inputView.Rows[cell.RowIndex].Selected = true;
                }

                DataGridViewSelectedRowCollection selectedRows = inputView.SelectedRows;

                //inputView.ClearSelection();
                try
                {
                    inputView.Rows[inputView.HitTest(e.X, e.Y).RowIndex].Selected = true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return;
                }
                
                // Open right-click table context menu
                ContextMenu menu = new ContextMenu();
                MenuItem cut = new MenuItem("Cut");
                MenuItem copy = new MenuItem("Copy");
                MenuItem paste = new MenuItem("Paste");
                MenuItem insert = new MenuItem("Insert Frame");
                cut.Click += cut_Data;
                copy.Click += copy_Data;
                paste.Click += paste_Data;
                insert.Click += insert_BlankFrame;
                menu.MenuItems.Add(cut);
                menu.MenuItems.Add(copy);
                menu.MenuItems.Add(paste);
                menu.MenuItems.Add(insert);

                menu.Show(inputView, new Point(e.X, e.Y));
            }
        }

        private void cut_Data(object sender, EventArgs e)
        {
            // First copy data
            copy_Data(sender, e);

            int firstIndex = inputView.SelectedRows[0].Index;
            // Remove row(s)
            foreach (DataGridViewRow row in inputView.SelectedRows)
            {
                inputView.Rows.RemoveAt(row.Index);
            }

            // Re-synchronize frame column
            resync_FrameCount(firstIndex);
        }

        private void copy_Data(object sender, EventArgs e)
        {
            // Parse rows into a comma, separated string
            string data = "";
            
            // Get selected rows and iterate through each cell to parse the value of cells to a string
            DataGridViewSelectedRowCollection selectedRows = inputView.SelectedRows;
            foreach (DataGridViewRow row in selectedRows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.GetType() == typeof(DataGridViewCheckBoxCell))
                    {
                        if (cell.Value != DBNull.Value)
                        {
                         data = data + "true, ";
                         var colIndex = cell.ColumnIndex;
                         string colName = inputView.Columns[colIndex].Name;
                        }
                        else data = data + "false, ";
                    }
                    else data = data + cell.Value.ToString() + ",";
                }

                data = data.Substring(0, data.Length - 2) + ";";
            }
            data = data.Substring(0, data.Length - 2);

            Clipboard.SetText(data);
        }

        private void paste_Data(object sender, EventArgs e)
        {
            string data = Clipboard.GetText();
            string[] dataRows = data.Split(';');
            
            // Functionality right now: Will overwrite the table at the selected index, adding new rows if it exceeds the length of the table
            // We may want to change this to insert in the table at a later time

            // Determine where to paste this row
            int rowIndex = inputView.SelectedRows[0].Index;

            // Iterate each frame
            for (int i = 0; i < dataRows.Length; i++)
            {
                DataRow rowData = table.NewRow();
                string[] dataCells = dataRows[i].Split(',');

                // Iterate each cell
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    rowData[j] = dataCells[j];
                }

                table.Rows.InsertAt(rowData, rowIndex + i);
            }

            // Resynchronize frame column
            resync_FrameCount(rowIndex);
        }

        private void insert_BlankFrame(object sender, EventArgs e)
        {
            table.Rows.InsertAt(tableGenerateDefaultRow(), inputView.SelectedRows[0].Index);
            resync_FrameCount(inputView.SelectedRows[0].Index - 1);
        }

        private void resync_FrameCount(int index)
        {
            // In the event that a frame is inserted or deleted, we need to resynchronize the Frame column
            // Frame is correct up until index
            // From index onwards, set the Frame cell based on the previous row's value
            for (int i = index; i < inputView.Rows.Count; i++)
            {
                inputView.Rows[i].Cells["Frame"].Value = i + 1;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (inputView.SelectedRows.Count == 0) return;

                // In case multiple rows are selected, just focus on last frame
                int lastIndex = inputView.SelectedRows[inputView.SelectedRows.Count - 1].Index;
                
                DataRow newRow;
                newRow = table.NewRow();
                newRow[0] = lastIndex + 2;
                
                for (int i = 1; i < table.Columns.Count; i++)
                {
                    newRow[i] = 0;
                }
                //newRow.ItemArray = [lastIndex + 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
                //table.Rows.InsertAt(, lastIndex + 1);
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void inputView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            inputView.Rows[inputView.Rows.Count - 1].Cells["Frame"].Value = table.Rows.Count + 1;
            for (int i = 0; i < inputView.Columns.Count; i++)
            {
                // If the cell is an analog field and it was not modified, set it to default 128
                if (table.Columns[i].DataType == System.Type.GetType("System.Int16") && inputView.Rows[inputView.Rows.Count - 1].Cells[i].Value == null && table.Columns[i].ColumnName != "Frame")
                {
                    inputView.Rows[inputView.Rows.Count - 1].Cells[i].Value = 128;
                }
            }
        }

        private void inputView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Handle the case where checkbox cell value changes are not instantly reflected
            if (inputView.IsCurrentCellDirty) inputView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void inputView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            inputView.Rows[e.RowIndex].Selected = true;
        }
    }
}
