/* TODO:
 * implement save/open
 * implement save before new
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
    public partial class TAStudio : Form
    {
        private DataTable table = new DataTable();
        private string fileName = "";
        public bool Changed
        {
            get { return _changed; }
            set
            {
                this._changed = value;
                if (this._changed)
                {
                    saveToolStripMenuItem.Enabled = true;
                }
            }
        }
        private bool _changed = false;
        private bool dataCopiedToClipboard = false;

        // Represent columns in a list of tuples, to allow for easy modification should these column names change
        private readonly List<(string, string)> columnNames = new List<(string, string)>()
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

        public TAStudio()
        {
            InitializeComponent();
            DisableMenuButtons();
        }

        #region Table format-related functions

        private void TableGenerateColumns()
        {
            foreach ((string, string) column in columnNames)
            {
                if (column.Item2 == "Int") table.Columns.Add(column.Item1, typeof(Int16));
                else if (column.Item2 == "Bool") table.Columns.Add(column.Item1, typeof(bool));
            }
        }

        private void ResizeInputViewColumns()
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

        private void InputView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Handle the case where checkbox cell value changes are not instantly reflected
            if (inputView.IsCurrentCellDirty) inputView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            Changed = true;
        }

        private void InputView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            inputView.Rows[e.RowIndex].Selected = true;
            cutCtrlXToolStripMenuItem.Enabled = true;
            copyToolStripMenuItem.Enabled = true;
        }
        #endregion

        #region I/O Functions
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // On Enter, move down row or generate new row
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
        private void InputView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
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

                // If multiple cells are selected, then highlight the entire row of all cells
                foreach (DataGridViewCell cell in inputView.SelectedCells)
                {
                    inputView.Rows[cell.RowIndex].Selected = true;
                }

                // Open right-click table context menu
                ContextMenu menu = new ContextMenu();
                MenuItem cut = new MenuItem("Cut");
                MenuItem copy = new MenuItem("Copy");
                MenuItem paste = new MenuItem("Paste");
                MenuItem insert = new MenuItem("Insert Frame");
                MenuItem pasteInsert = new MenuItem("Paste Insert");

                // Check for data in clipboard for paste operations
                if (!dataCopiedToClipboard)
                {
                    paste.Enabled = false;
                    pasteInsert.Enabled = false;
                }
                cut.Click += Cut_Data;
                copy.Click += Copy_Data;
                paste.Click += Paste_Data;
                insert.Click += Insert_BlankFrame;
                pasteInsert.Click += PasteInsert_Data;
                menu.MenuItems.Add(cut);
                menu.MenuItems.Add(copy);
                menu.MenuItems.Add(paste);
                menu.MenuItems.Add(insert);
                menu.MenuItems.Add(pasteInsert);

                menu.Show(inputView, new Point(e.X, e.Y));
            }
        }
        #endregion

        #region Functions relating to new row creation

        private DataRow TableGenerateDefaultRow()
        {
            DataRow newRow = table.NewRow();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == "Frame")
                {
                    newRow[i] = table.Rows.Count + 1;
                }
                else if (table.Columns[i].DataType == System.Type.GetType("System.Int16")) newRow[i] = 128;
            }

            return newRow;
        }

        private void InputView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
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

        private void Insert_BlankFrame(object sender, EventArgs e)
        {
            table.Rows.InsertAt(TableGenerateDefaultRow(), inputView.SelectedRows[0].Index);
            MessageBox.Show(inputView.SelectedRows[0].Index.ToString());
            Resync_FrameCount(inputView.SelectedRows[0].Index - 1);
        }

        private void Resync_FrameCount(int index)
        {
            // In the event that a frame is inserted or deleted, we need to resynchronize the Frame column
            // Frame is correct up until index
            // From index onwards, set the Frame cell based on the previous row's value
            for (int i = index; i < inputView.Rows.Count; i++)
            {
                inputView.Rows[i].Cells["Frame"].Value = i + 1;
            }
        }

        #endregion

        #region Menu buttons
        private void DisableMenuButtons()
        {
            cutCtrlXToolStripMenuItem.Enabled = false;
            copyToolStripMenuItem.Enabled = false;
            pasteToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            undoToolStripMenuItem.Enabled = false;
            redoToolStripMenuItem.Enabled = false;
            closeToolStripMenuItem.Enabled = false;
            pasteInsertToolStripMenuItem.Enabled = false;
            insertFrameToolStripMenuItem.Enabled = false;
            insertFramesToolStripMenuItem.Enabled = false;
        }
        private void NewCtrlNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Save before quitting an open table

            // Disable menu buttons
            DisableMenuButtons();
            //Enable close and saveAs
            closeToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;

            // Generate a blank table with one default row
            TableGenerateColumns();
            table.Rows.Add(TableGenerateDefaultRow());
            inputView.DataSource = table;
            ResizeInputViewColumns();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Prompt the user to save
            if (fileName == "")
            {
                SaveFileDialog save = new SaveFileDialog
                {
                    Title = "Save Opened TAStudio Frame Table",
                    Filter = "TAStudio Frame Table|*.tas"
                };
                save.ShowDialog();

                if (save.FileName != "")
                {
                    Save_Data(save.FileName);
                }
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement save before closing
            // if (changed)....

            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                ClearDataTable();
                fileName = open.FileName;
                Open_Data(fileName);
            }
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDataTable();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy_Data(sender, e);
            pasteToolStripMenuItem.Enabled = true;
        }

        private void CutCtrlXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cut_Data(sender, e);
            pasteToolStripMenuItem.Enabled = true;
        }

        private void PasteCtrlVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste_Data(sender, e);
        }

        #endregion

        #region Data functions

        private void Save_Data(string fileLocation)
        {
            string data = "";

            using (StreamWriter file = new StreamWriter(fileLocation))
            {
                // First write column headers to the file
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    data += table.Columns[i].ColumnName + ",";
                }
                data = data.Substring(0, data.Length - 1);

                file.WriteLine(data);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    DataRow row = table.Rows[i];
                    data = Parse_Data(row);
                    data = data.Substring(0, data.Length - 1);

                    file.WriteLine(data);
                }
            }
        }
        private void Open_Data(string fileLocation)
        {
            // Read from file and assert that it is formatted properly
            using (StreamReader reader = new StreamReader(fileLocation))
            {
                string data;

                while (!reader.EndOfStream)
                {
                    data = reader.ReadLine();
                }
            }
        }

        private void ClearDataTable()
        {
            if (Changed)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to clear the table?", "Close", MessageBoxButtons.YesNo);
                if (result == DialogResult.No) return;
            }
            table.Columns.Clear();
            table.Clear();
            DisableMenuButtons();
            fileName = null;
            Changed = false;
        }

        private string Parse_Data(DataRow row)
        {
            string parsedData = "";
            for (int j = 0; j < table.Columns.Count; j++)
            {
                object cell = row.ItemArray[j];

                if (cell.GetType() == typeof(Int16))
                {
                    parsedData = parsedData + ((Int16)cell).ToString() + ",";
                }
                else if (cell.GetType() == typeof(DBNull))
                {
                    parsedData += "false,";
                }
                else if (cell.GetType() == typeof(bool))
                {
                    parsedData = (bool)cell ? parsedData += "true," : parsedData += "false,";
                }
            }

            return parsedData.Substring(0, parsedData.Length - 1) + ";";
        }

        private void Cut_Data(object sender, EventArgs e)
        {
            // First copy data
            Copy_Data(sender, e);

            int firstIndex = inputView.SelectedRows[0].Index;
            // Remove row(s)
            foreach (DataGridViewRow row in inputView.SelectedRows)
            {
                inputView.Rows.RemoveAt(row.Index);
            }

            // Re-synchronize frame column
            Resync_FrameCount(firstIndex);

            dataCopiedToClipboard = true;
            Changed = true;
        }

        private void Copy_Data(object sender, EventArgs e)
        {
            // Parse rows into a comma, separated string
            string data = "";

            // Get selected rows and iterate through each cell to parse the value of cells to a string
            for (int i = 0; i < inputView.SelectedRows.Count; i++)
            {
                // Handle the instance when the user tries to copy the uncommitted new row
                if (inputView.SelectedRows[i].Cells[0].Value == null)
                {
                    data = Parse_Data(TableGenerateDefaultRow());
                }
                else
                {
                    int rowIndex = inputView.SelectedRows[i].Index;
                    data = Parse_Data(table.Rows[rowIndex]);
                }
            }
            data = data.Substring(0, data.Length - 1);

            Clipboard.SetText(data);

            dataCopiedToClipboard = true;
            pasteToolStripMenuItem.Enabled = true;
        }

        private void Paste_Data(object sender, EventArgs e)
        {
            string data = Clipboard.GetText();
            string[] rowsData = data.Split(';');

            // Determine where to paste this row
            int rowIndex = inputView.SelectedRows[0].Index;

            // To overwrite rows, delete rows first
            // CURRENT FUNCTIONALITY: Deletes all rows that are selected upon pasting
            while (inputView.SelectedRows.Count > 0)
            {
                try
                {
                    inputView.Rows.RemoveAt(inputView.SelectedRows[0].Index);
                }
                catch (InvalidOperationException)
                {
                    // The user right clicked on the newest row... Just insert the row to the end
                    break;
                }
            }

            // Iterate each frame
            for (int i = 0; i < rowsData.Length; i++)
            {
                DataRow rowData = table.NewRow();
                string[] cellsData = rowsData[i].Split(',');

                // Iterate each cell
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    rowData[j] = cellsData[j];
                }

                table.Rows.InsertAt(rowData, rowIndex + i);
            }

            Changed = true;

            // Resynchronize frame column
            Resync_FrameCount(rowIndex);
        }

        private void PasteInsert_Data(object sender, EventArgs e)
        {
            string data = Clipboard.GetText();
            string[] dataRows = data.Split(';');

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

            Changed = true;

            // Resynchronize frame column
            Resync_FrameCount(rowIndex);
        }

        #endregion
    }
}
