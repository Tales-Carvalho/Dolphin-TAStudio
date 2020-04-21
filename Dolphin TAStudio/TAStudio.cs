/* TODO:
 * On table modification, clear and re-set the queue
 * In order to clear queue on table modification or savestate load, clear InputsInQueue in Dolphin code
 * Implement keyboard shortcuts
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
using Microsoft.VisualBasic;

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
                    if (this.Text.IndexOf("*") == -1) { this.Text = this.Text.Insert(this.Text.IndexOf("-") + 2, "*"); }
                }
                else
                {
                    saveToolStripMenuItem.Enabled = false;
                }
            }
        }
        private bool _changed = false;
        private bool dataCopiedToClipboard = false;
        private bool sendInputsToDolphin = false; // Determines whether or not to send inputs to the emulator, based on whether or not it is in read-only mode

        // Represent columns in a list of tuples, to allow for easy modification should these column names change
        private readonly List<(string, string)> columnNames = new List<(string, string)>()
        {
            ("Save?", "Bool"),
            ("Frame", "Byte"),
            ("aX", "Byte"),
            ("aY", "Byte"),
            ("A", "Bool"),
            ("B", "Bool"),
            ("X", "Bool"),
            ("Y", "Bool"),
            ("S", "Bool"),
            ("Z", "Bool"),
            ("L", "Bool"),
            ("R", "Bool"),
            ("La", "Byte"),
            ("Ra", "Byte"),
            ("dU", "Bool"),
            ("dD", "Bool"),
            ("dL", "Bool"),
            ("dR", "Bool"),
            ("cX", "Byte"),
            ("cY", "Byte")
        };

        DolphinMemoryInterface dmi;
        DolphinMemoryInterface.GCController gcCont = new DolphinMemoryInterface.GCController();

        public TAStudio()
        {
            InitializeComponent();
            DisableMenuButtons();
            
            try
            {
                dmi = new DolphinMemoryInterface();

                dmi.MoviePlaybackStateChanged += checkReadOnly;
                dmi.InputFrameCountChanged += OnFrameUpdate;

                checkReadOnly(null, null);
            }
            catch (Exception e)
            {
                // Show error message and activate a button to retry connection - similar to how it's done in DolphinMemoryEngine
            }
        }

        #region Table format-related functions

        private void TableGenerateColumns()
        {
            foreach ((string, string) column in columnNames)
            {
                if (column.Item2 == "Byte") table.Columns.Add(column.Item1, typeof(Byte));
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
            cutToolStripMenuItem.Enabled = true;
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
            else if (e.Control)
            {
                if (e.KeyCode == Keys.O) { OpenToolStripMenuItem_Click(sender, e); }
                else if (e.KeyCode == Keys.S) { SaveToolStripMenuItem_Click(sender, e); }
                else if (e.KeyCode == Keys.N) { NewToolStripMenuItem_Click(sender, e); }
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
                else if (table.Columns[i].ColumnName == "La" || table.Columns[i].ColumnName == "Ra") newRow[i] = 0;
                else if (table.Columns[i].DataType == System.Type.GetType("System.Byte")) newRow[i] = 128;
            }

            return newRow;
        }

        private void InputView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            inputView.Rows[inputView.Rows.Count - 1].Cells["Frame"].Value = table.Rows.Count + 1;
            for (int i = 0; i < inputView.Columns.Count; i++)
            {
                // If the cell is an analog field and it was not modified, set it to default 128
                if (table.Columns[i].DataType == System.Type.GetType("System.Byte") && inputView.Rows[inputView.Rows.Count - 1].Cells[i].Value == null && table.Columns[i].ColumnName != "Frame")
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
            cutToolStripMenuItem.Enabled = false;
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
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDataTable();
            // Check to see if the table was cleared. If not, then this means that we did not save changes and should avoid generating a new table
            if (table.Rows.Count > 0) { return; }

            this.Text = "Dolphin TAStudio - Untitled";
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
                SaveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                Save_Data(fileName);
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog
            {
                Title = "Save Opened TAStudio Frame Table",
                Filter = "TAStudio Frame Table|*.tas"
            };
            save.ShowDialog();
            if (save.FileName == "") return;

            Save_Data(save.FileName);
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement save before closing
            // if (changed)....

            OpenFileDialog open = new OpenFileDialog
            {
                Title = "Open TAStudio Frame Table",
                Filter = "TAStudio Frame Table|*.tas",
                Multiselect = false
            };

            if (open.ShowDialog() == DialogResult.OK)
            {
                ClearDataTable();
                Open_Data(open.FileName);
                this.Text = "Dolphin TAStudio - " + Path.GetFileName(open.FileName);

                // Parse table data into gcCont objects
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    dmi.AddInputInQueue(parseTableInputs(inputView.Rows[i]));
                    if (i == 0) { dmi.AddInputInQueue(parseTableInputs(inputView.Rows[i])); } // Resend the first frame so that it processes correctly

                }
            }
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDataTable();
            if (_changed) { return; } // If the file was not saved, do not close the table
            dmi.DeactivateInputs();
            this.Text = "Dolphin TAStudio";
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy_Data(sender, e);
            pasteToolStripMenuItem.Enabled = true;
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cut_Data(sender, e);
            pasteToolStripMenuItem.Enabled = true;
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste_Data(sender, e);
        }

        #endregion

        #region Data functions

        private void Save_Data(string fileLocation)
        {
            string data = "";

            try
            {
                using (FileStream fileStream = File.Open(fileLocation, FileMode.Open))
                {
                    fileStream.SetLength(0);
                    fileStream.Close();
                }
            }
            catch (FileNotFoundException) { ; }

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

            fileName = Path.GetFileName(fileLocation);
            this.Text = "Dolphin TAStudio - " + Path.GetFileName(fileName);
            Changed = false;
        }

        private void Open_Data(string fileLocation)
        {
            // Read from file and assert that it is formatted properly
            using (StreamReader reader = new StreamReader(fileLocation))
            {
                string data;
                data = reader.ReadLine(); // Column names

                // Assert that the number of columns is the same as the number of intended columns
                string[] dataColumnNames = data.Split(',');
                if (dataColumnNames.Length != columnNames.Count)
                {
                    MessageBox.Show("Cannot open file: Number of columns is incorrect.", "Open File Error", MessageBoxButtons.OK);
                    return;
                }
                for (int i = 0; i < columnNames.Count; i++)
                {
                    if (columnNames[i].Item1 != dataColumnNames[i])
                    {
                        MessageBox.Show("Cannot open file: Unexpected column name: " + columnNames[i].Item1, "Open File Error", MessageBoxButtons.OK);
                        return;
                    }
                }

                TableGenerateColumns();

                // Column names match. Begin reading data
                while (!reader.EndOfStream)
                {
                    data = reader.ReadLine();
                    DataRow newRow = table.NewRow();

                    string[] cellData = data.Split(',');

                    for (int i = 0; i < columnNames.Count; i++)
                    {
                        if (columnNames[i].Item2 == "Byte")
                        {
                            // If the file's integer cell value is < 0 or > 255, then fix
                            if (Convert.ToByte(cellData[i]) < 0)
                            {
                                newRow[table.Columns[i]] = 0;
                            }
                            else if (Convert.ToByte(cellData[i]) > 255)
                            {
                                newRow[table.Columns[i]] = 255;
                            }
                            else
                            {
                                newRow[table.Columns[i]] = cellData[i];
                            }
                        }
                        else if (columnNames[i].Item2 == "Bool")
                        {
                            if (cellData[i] == "true")
                            {
                                newRow[table.Columns[i]] = true;
                            }
                            else if (cellData[i] == "false")
                            {
                                newRow[table.Columns[i]] = false;
                            }
                        }
                    }

                    table.Rows.Add(newRow);
                }

                inputView.DataSource = table;
            }

            ResizeInputViewColumns();
            Resync_FrameCount(0);
            dmi.ActivateInputs();

            // Enable close and save as menu buttons
            closeToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;

            fileName = Path.GetFileName(fileLocation);
        }

        private void ClearDataTable()
        {
            if (Changed)
            {
                DialogResult result = MessageBox.Show("Do you want to save before closing?", "Close Table", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel) { return; }
                if (result == DialogResult.Yes)
                {
                    if (fileName == "")
                    {
                        SaveAsToolStripMenuItem_Click(null, null);
                        if (fileName == "") { return; } // If file is not saved, do not clear the table
                    }
                    else { SaveToolStripMenuItem_Click(null, null); }

                }
            }
            table.Columns.Clear();
            table.Clear();
            DisableMenuButtons();
            fileName = "";
            Changed = false;
            dmi.DeactivateInputs();
            this.Text = "Dolphin TAStudio";
        }

        private string Parse_Data(DataRow row)
        {
            string parsedData = "";
            for (int j = 0; j < table.Columns.Count; j++)
            {
                object cell = row.ItemArray[j];

                if (cell.GetType() == typeof(Byte))
                {
                    parsedData = parsedData + ((Byte)cell).ToString() + ",";
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

        #region Dolphin-related Functions

        private void checkReadOnly(object sender, EventArgs e)
        {
            if (dmi.IsMoviePlayingBack())
            {
                if (recordDolphinInputs.Checked == false)
                {
                    readOnlyWarning.Visible = true;
                }
                sendInputsToDolphin = false;
            }
            else
            {
                sendInputsToDolphin = true;
                readOnlyWarning.Visible = false;
            }
        }
        
        private void FrameAdvance_Click(object sender, EventArgs e)
        {
            dmi.FrameAdvance();
        }

        private void Play_Click(object sender, EventArgs e)
        {
            dmi.PlayEmulation();
        }

        private void Pause_Click(object sender, EventArgs e)
        {
            dmi.PauseEmulation();
        }

        private void Savestate_Click(object sender, EventArgs e)
        {
            dmi.SaveState();
        }

        private void LoadState_Click(object sender, EventArgs e)
        {
            dmi.LoadState();
        }

        private void SetStateName_Click(object sender, EventArgs e)
        {
            string name = Interaction.InputBox("Enter a savestate name", "Set Savestate Name", "", 0, 0);
            //if ((name.Substring(name.Length - 5)) != ".dtm") name += ".dtm";

            dmi.SetStateName(name);
        }
        
        // Turn data at frame into gcCont inputs
        private DolphinMemoryInterface.GCController parseTableInputs(DataGridViewRow frameData)
        {
            DolphinMemoryInterface.GCController gcInput = new DolphinMemoryInterface.GCController();
            gcInput.button = 0;

            for (int i = 0; i < frameData.Cells.Count; i++)
            {
                string colName = columnNames[i].Item1;
                if (colName == "A" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_A; }
                else if (colName == "B" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_B; }
                else if (colName == "X" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_X; }
                else if (colName == "Y" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_Y; }
                else if (colName == "L" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_TRIGGER_L; }
                else if (colName == "R" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_TRIGGER_R; }
                else if (colName == "Z" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_Z; }
                else if (colName == "S" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_START; }
                else if (colName == "dU" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_UP; }
                else if (colName == "dD" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_DOWN; }
                else if (colName == "dL" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_LEFT; }
                else if (colName == "dR" && (bool)(frameData.Cells[i].Value)) { gcInput.button |= (ushort)DolphinMemoryInterface.PadButton.PAD_BUTTON_RIGHT; }
                else if (colName == "aX") { gcInput.stickX = (byte)frameData.Cells[i].Value; }
                else if (colName == "aY") { gcInput.stickY = (byte)frameData.Cells[i].Value; }
                else if (colName == "cX") { gcInput.substickX = (byte)frameData.Cells[i].Value; }
                else if (colName == "cY") { gcInput.substickY = (byte)frameData.Cells[i].Value; }
                else if (colName == "La") { gcInput.triggerLeft = (byte)frameData.Cells[i].Value; }
                else if (colName == "Ra") { gcInput.triggerRight = (byte)frameData.Cells[i].Value; }
            }

            return gcInput;
        }

        private void OnFrameUpdate(object sender, EventArgs e)
        {

        }
        #endregion
    }
}
