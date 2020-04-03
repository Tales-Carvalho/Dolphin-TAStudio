/* TODO: 
- If you click and drag between the last row and the new row, Frame will increment and break
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
        private int frameCount = 1;

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
                frameCount = 0;

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
                else if (column.Item2 == "Bool") table.Columns.Add(column.Item1, typeof(Boolean));
            }
        }

        private void tableGenerateDefaultRow()
        {
            DataRow newRow = table.NewRow();
            for (int i = 1; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == "Frame")
                {
                    newRow[i] = frameCount;
                    frameCount++;
                }
                else if (table.Columns[i].DataType == System.Type.GetType("System.Int16")) newRow[i] = 128;
            }

            table.Rows.Add(newRow);
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
            tableGenerateDefaultRow();
            inputView.DataSource = table;
            resizeInputViewColumns();
        }

        private void inputView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // If the user right clicks on a row, select that row
                inputView.ClearSelection();
                inputView.Rows[inputView.HitTest(e.X, e.Y).RowIndex].Selected = true;
                
                // Open right-click table context menu
                ContextMenu menu = new ContextMenu();
                MenuItem cut = new MenuItem("Cut Frame");
                MenuItem copy = new MenuItem("Copy Frame");
                MenuItem paste = new MenuItem("Paste Frame");
                cut.Click += cut_Row;
                copy.Click += copy_Row;
                paste.Click += paste_Row;

                int currentMouseOverRow = inputView.HitTest(e.X, e.Y).RowIndex;

                menu.Show(inputView, new Point(e.X, e.Y));
            }
        }

        private void cut_Row(object sender, EventArgs e)
        {

        }

        private void copy_Row(object sender, EventArgs e)
        {

        }

        private void paste_Row(object sender, EventArgs e)
        {

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

        private void inputView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            inputView.Rows[inputView.Rows.Count - 1].Cells["Frame"].Value = frameCount;
            frameCount++;
            for (int i = 0; i < inputView.Columns.Count; i++)
            {
                // If the cell is an analog field and it was not modified, set it to default 128
                if (table.Columns[i].DataType == System.Type.GetType("System.Int16") && inputView.Rows[inputView.Rows.Count - 1].Cells[i].Value == null && table.Columns[i].ColumnName != "Frame")
                {
                    inputView.Rows[inputView.Rows.Count - 1].Cells[i].Value = 128;
                }
            }
        }
    }
}
