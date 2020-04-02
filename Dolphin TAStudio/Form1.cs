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
        private int frameCount;

        // Represent columns in a list of tuples, to allow for easy modification should these column names change
        private List<(string, string)> columnNames = new List<(string, string)>()
        {
            ("Frame", "Integer"),
            ("aX", "Integer"),
            ("aY", "Intger"),
            ("A", "Bool"),
            ("B", "Bool"),
            ("X", "Bool"),
            ("Y", "Bool"),
            ("Z", "Bool"),
            ("L", "Bool"),
            ("R", "Bool"),
            ("La", "Integer"),
            ("Ra", "Integer"),
            ("dU", "Bool"),
            ("dD", "Bool"),
            ("dL", "Bool"),
            ("dR", "Bool"),
            ("cX", "Integer"),
            ("cY", "Integer")
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

        }

        private void newCtrlNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Save before quitting an open table

            // Generate a blank table for a new recording file
            table.Columns.Add("Frame", typeof(int));
            table.Columns[0].ReadOnly = true;
            table.Columns.Add("aX", typeof(int));
            table.Columns.Add("aY", typeof(int));
            table.Columns.Add("A", typeof(bool));
            table.Columns.Add("B", typeof(bool));
            table.Columns.Add("X", typeof(bool));
            table.Columns.Add("Y", typeof(bool));
            table.Columns.Add("Z", typeof(bool));
            table.Columns.Add("L", typeof(bool));
            table.Columns.Add("R", typeof(bool));
            table.Columns.Add("La", typeof(bool));
            table.Columns.Add("Ra", typeof(bool));
            table.Columns.Add("u", typeof(bool));
            table.Columns.Add("d", typeof(bool));
            table.Columns.Add("l", typeof(bool));
            table.Columns.Add("r", typeof(bool));
            table.Columns.Add("cX", typeof(int));
            table.Columns.Add("cY", typeof(int));
            table.Rows.Add(1, 128, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 128);

            inputView.DataSource = table;
            inputView.Columns[0].Width = 60;
            inputView.Columns[1].Width = 30;
            inputView.Columns[2].Width = 30;
            inputView.Columns[3].Width = 30;
            inputView.Columns[4].Width = 30;
            inputView.Columns[5].Width = 30;
            inputView.Columns[6].Width = 30;
            inputView.Columns[7].Width = 30;
            inputView.Columns[8].Width = 30;
            inputView.Columns[9].Width = 30;
            inputView.Columns[10].Width = 30;
            inputView.Columns[11].Width = 30;
            inputView.Columns[12].Width = 30;
            inputView.Columns[13].Width = 30;
            inputView.Columns[14].Width = 30;
            inputView.Columns[15].Width = 30;
            inputView.Columns[16].Width = 30;
            inputView.Columns[17].Width = 30;
        }

        private void inputView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu menu = new ContextMenu();
                menu.MenuItems.Add(new MenuItem("Cut Frame"));
                menu.MenuItems.Add(new MenuItem("Copy Frame"));
                menu.MenuItems.Add(new MenuItem("Paste Frame"));

                int currentMouseOverRow = inputView.HitTest(e.X, e.Y).RowIndex;

                menu.Show(inputView, new Point(e.X, e.Y));
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
    }
}
