namespace Dolphin_TAStudio
{
    partial class TAStudio
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertFramesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteInsertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inputView = new System.Windows.Forms.DataGridView();
            this.playbackBox = new System.Windows.Forms.GroupBox();
            this.SetStateName = new System.Windows.Forms.Button();
            this.LoadState = new System.Windows.Forms.Button();
            this.Savestate = new System.Windows.Forms.Button();
            this.Pause = new System.Windows.Forms.Button();
            this.Play = new System.Windows.Forms.Button();
            this.FrameAdvance = new System.Windows.Forms.Button();
            this.readOnlyWarning = new System.Windows.Forms.TextBox();
            this.recordDolphinInputs = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputView)).BeginInit();
            this.playbackBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(869, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.newToolStripMenuItem.Text = "New (Ctrl + N)";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.openToolStripMenuItem.Text = "Open (Ctrl + O)";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.saveToolStripMenuItem.Text = "Save (Ctrl + S)";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.saveAsToolStripMenuItem.Text = "Save As (Ctrl + Shift + S)";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.insertFrameToolStripMenuItem,
            this.insertFramesToolStripMenuItem,
            this.pasteInsertToolStripMenuItem,
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.cutToolStripMenuItem.Text = "Cut (Ctrl + X)";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.CutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyToolStripMenuItem.Text = "Copy (Ctrl + C)";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pasteToolStripMenuItem.Text = "Paste (Ctrl + V)";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.PasteToolStripMenuItem_Click);
            // 
            // insertFrameToolStripMenuItem
            // 
            this.insertFrameToolStripMenuItem.Name = "insertFrameToolStripMenuItem";
            this.insertFrameToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertFrameToolStripMenuItem.Text = "Insert Frame";
            // 
            // insertFramesToolStripMenuItem
            // 
            this.insertFramesToolStripMenuItem.Name = "insertFramesToolStripMenuItem";
            this.insertFramesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.insertFramesToolStripMenuItem.Text = "Insert Frames";
            // 
            // pasteInsertToolStripMenuItem
            // 
            this.pasteInsertToolStripMenuItem.Name = "pasteInsertToolStripMenuItem";
            this.pasteInsertToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pasteInsertToolStripMenuItem.Text = "Paste Insert";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.undoToolStripMenuItem.Text = "Undo (Ctrl + Z)";
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.redoToolStripMenuItem.Text = "Redo (Ctrl + Y)";
            // 
            // inputView
            // 
            this.inputView.AllowUserToResizeColumns = false;
            this.inputView.AllowUserToResizeRows = false;
            this.inputView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.inputView.Location = new System.Drawing.Point(12, 27);
            this.inputView.Name = "inputView";
            this.inputView.Size = new System.Drawing.Size(633, 563);
            this.inputView.TabIndex = 1;
            this.inputView.VirtualMode = true;
            this.inputView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.InputView_CellClick);
            this.inputView.CurrentCellDirtyStateChanged += new System.EventHandler(this.InputView_CurrentCellDirtyStateChanged);
            this.inputView.NewRowNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.InputView_NewRowNeeded);
            this.inputView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.InputView_MouseClick);
            // 
            // playbackBox
            // 
            this.playbackBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackBox.Controls.Add(this.SetStateName);
            this.playbackBox.Controls.Add(this.LoadState);
            this.playbackBox.Controls.Add(this.Savestate);
            this.playbackBox.Controls.Add(this.Pause);
            this.playbackBox.Controls.Add(this.Play);
            this.playbackBox.Controls.Add(this.FrameAdvance);
            this.playbackBox.Location = new System.Drawing.Point(651, 27);
            this.playbackBox.Name = "playbackBox";
            this.playbackBox.Size = new System.Drawing.Size(206, 110);
            this.playbackBox.TabIndex = 2;
            this.playbackBox.TabStop = false;
            this.playbackBox.Text = "Playback";
            // 
            // SetStateName
            // 
            this.SetStateName.Location = new System.Drawing.Point(108, 18);
            this.SetStateName.Name = "SetStateName";
            this.SetStateName.Size = new System.Drawing.Size(92, 23);
            this.SetStateName.TabIndex = 5;
            this.SetStateName.Text = "Set State Name";
            this.SetStateName.UseVisualStyleBackColor = true;
            this.SetStateName.Click += new System.EventHandler(this.SetStateName_Click);
            // 
            // LoadState
            // 
            this.LoadState.Location = new System.Drawing.Point(107, 77);
            this.LoadState.Name = "LoadState";
            this.LoadState.Size = new System.Drawing.Size(93, 23);
            this.LoadState.TabIndex = 4;
            this.LoadState.Text = "Load State";
            this.LoadState.UseVisualStyleBackColor = true;
            this.LoadState.Click += new System.EventHandler(this.LoadState_Click);
            // 
            // Savestate
            // 
            this.Savestate.Location = new System.Drawing.Point(107, 48);
            this.Savestate.Name = "Savestate";
            this.Savestate.Size = new System.Drawing.Size(93, 23);
            this.Savestate.TabIndex = 3;
            this.Savestate.Text = "Savestate";
            this.Savestate.UseVisualStyleBackColor = true;
            this.Savestate.Click += new System.EventHandler(this.Savestate_Click);
            // 
            // Pause
            // 
            this.Pause.Location = new System.Drawing.Point(7, 77);
            this.Pause.Name = "Pause";
            this.Pause.Size = new System.Drawing.Size(94, 23);
            this.Pause.TabIndex = 2;
            this.Pause.Text = "Pause";
            this.Pause.UseVisualStyleBackColor = true;
            this.Pause.Click += new System.EventHandler(this.Pause_Click);
            // 
            // Play
            // 
            this.Play.Location = new System.Drawing.Point(6, 48);
            this.Play.Name = "Play";
            this.Play.Size = new System.Drawing.Size(95, 23);
            this.Play.TabIndex = 1;
            this.Play.Text = "Play";
            this.Play.UseVisualStyleBackColor = true;
            this.Play.Click += new System.EventHandler(this.Play_Click);
            // 
            // FrameAdvance
            // 
            this.FrameAdvance.Location = new System.Drawing.Point(6, 19);
            this.FrameAdvance.Name = "FrameAdvance";
            this.FrameAdvance.Size = new System.Drawing.Size(95, 23);
            this.FrameAdvance.TabIndex = 0;
            this.FrameAdvance.Text = "Frame Advance";
            this.FrameAdvance.UseVisualStyleBackColor = true;
            this.FrameAdvance.Click += new System.EventHandler(this.FrameAdvance_Click);
            // 
            // readOnlyWarning
            // 
            this.readOnlyWarning.Location = new System.Drawing.Point(651, 167);
            this.readOnlyWarning.MaximumSize = new System.Drawing.Size(206, 200);
            this.readOnlyWarning.Multiline = true;
            this.readOnlyWarning.Name = "readOnlyWarning";
            this.readOnlyWarning.ReadOnly = true;
            this.readOnlyWarning.Size = new System.Drawing.Size(206, 35);
            this.readOnlyWarning.TabIndex = 3;
            this.readOnlyWarning.Text = "Warning! Dolphin is in read-only mode,\r\nand inputs here will not be processed.";
            // 
            // recordDolphinInputs
            // 
            this.recordDolphinInputs.AutoSize = true;
            this.recordDolphinInputs.Location = new System.Drawing.Point(652, 144);
            this.recordDolphinInputs.Name = "recordDolphinInputs";
            this.recordDolphinInputs.Size = new System.Drawing.Size(155, 17);
            this.recordDolphinInputs.TabIndex = 4;
            this.recordDolphinInputs.Text = "Record Inputs from Dolphin";
            this.recordDolphinInputs.UseVisualStyleBackColor = true;
            // 
            // TAStudio
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 602);
            this.Controls.Add(this.recordDolphinInputs);
            this.Controls.Add(this.readOnlyWarning);
            this.Controls.Add(this.playbackBox);
            this.Controls.Add(this.inputView);
            this.Controls.Add(this.menuStrip1);
            this.MinimumSize = new System.Drawing.Size(885, 39);
            this.Name = "TAStudio";
            this.Text = "Dolphin TAStudio";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputView)).EndInit();
            this.playbackBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.DataGridView inputView;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.GroupBox playbackBox;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteInsertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertFramesToolStripMenuItem;
        private System.Windows.Forms.Button FrameAdvance;
        private System.Windows.Forms.Button Play;
        private System.Windows.Forms.Button Pause;
        private System.Windows.Forms.Button Savestate;
        private System.Windows.Forms.Button LoadState;
        private System.Windows.Forms.Button SetStateName;
        private System.Windows.Forms.TextBox readOnlyWarning;
        private System.Windows.Forms.CheckBox recordDolphinInputs;
    }
}

