
namespace SpiritIsland.WinForms {

	partial class Form1 {
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.promptLabel = new System.Windows.Forms.Label();
            this.islandControl = new SpiritIsland.WinForms.IslandControl();
            this.cardControl = new SpiritIsland.WinForms.CardControl();
            this.spiritControl = new SpiritIsland.WinForms.SpiritControl();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.islandSpiritSplitter = new System.Windows.Forms.Splitter();
            this.textIslandSplitter = new System.Windows.Forms.Splitter();
            this.textPanel = new System.Windows.Forms.Panel();
            this.statusControl1 = new SpiritIsland.WinForms.StatusControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaySameGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.textPanel.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Location = new System.Drawing.Point(11, 9);
            this.promptLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(95, 15);
            this.promptLabel.TabIndex = 1;
            this.promptLabel.Text = "Decision Prompt";
            // 
            // islandControl
            // 
            this.islandControl.BackColor = System.Drawing.Color.Aqua;
            this.islandControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.islandControl.Location = new System.Drawing.Point(10, 35);
            this.islandControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.islandControl.Name = "islandControl";
            this.islandControl.Size = new System.Drawing.Size(658, 383);
            this.islandControl.TabIndex = 2;
            this.islandControl.Text = "islandControl1";
            // 
            // cardControl
            // 
            this.cardControl.BackColor = System.Drawing.Color.SaddleBrown;
            this.cardControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.cardControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardControl.Location = new System.Drawing.Point(0, 0);
            this.cardControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cardControl.Name = "cardControl";
            this.cardControl.Size = new System.Drawing.Size(1164, 254);
            this.cardControl.TabIndex = 3;
            this.cardControl.Text = "cardControl1";
            // 
            // spiritControl
            // 
            this.spiritControl.BackColor = System.Drawing.Color.LightYellow;
            this.spiritControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.spiritControl.Dock = System.Windows.Forms.DockStyle.Right;
            this.spiritControl.Location = new System.Drawing.Point(668, 35);
            this.spiritControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.spiritControl.Name = "spiritControl";
            this.spiritControl.Size = new System.Drawing.Size(496, 383);
            this.spiritControl.TabIndex = 8;
            this.spiritControl.Text = "spiritControl1";
            // 
            // splitContainer
            // 
            this.splitContainer.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 24);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.islandSpiritSplitter);
            this.splitContainer.Panel1.Controls.Add(this.islandControl);
            this.splitContainer.Panel1.Controls.Add(this.textIslandSplitter);
            this.splitContainer.Panel1.Controls.Add(this.spiritControl);
            this.splitContainer.Panel1.Controls.Add(this.textPanel);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.cardControl);
            this.splitContainer.Size = new System.Drawing.Size(1164, 676);
            this.splitContainer.SplitterDistance = 418;
            this.splitContainer.TabIndex = 10;
            // 
            // islandSpiritSplitter
            // 
            this.islandSpiritSplitter.Dock = System.Windows.Forms.DockStyle.Right;
            this.islandSpiritSplitter.Location = new System.Drawing.Point(651, 35);
            this.islandSpiritSplitter.Name = "islandSpiritSplitter";
            this.islandSpiritSplitter.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.islandSpiritSplitter.Size = new System.Drawing.Size(17, 383);
            this.islandSpiritSplitter.TabIndex = 9;
            this.islandSpiritSplitter.TabStop = false;
            // 
            // textIslandSplitter
            // 
            this.textIslandSplitter.Location = new System.Drawing.Point(0, 35);
            this.textIslandSplitter.Name = "textIslandSplitter";
            this.textIslandSplitter.Size = new System.Drawing.Size(10, 383);
            this.textIslandSplitter.TabIndex = 2;
            this.textIslandSplitter.TabStop = false;
            // 
            // textPanel
            // 
            this.textPanel.Controls.Add(this.promptLabel);
            this.textPanel.Cursor = System.Windows.Forms.Cursors.Default;
            this.textPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.textPanel.Location = new System.Drawing.Point(0, 0);
            this.textPanel.Name = "textPanel";
            this.textPanel.Size = new System.Drawing.Size(1164, 35);
            this.textPanel.TabIndex = 1;
            // 
            // statusControl1
            // 
            this.statusControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusControl1.Location = new System.Drawing.Point(0, 700);
            this.statusControl1.Name = "statusControl1";
            this.statusControl1.Size = new System.Drawing.Size(1164, 26);
            this.statusControl1.TabIndex = 11;
            this.statusControl1.Text = "statusControl1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1164, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.replaySameGameToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.gameToolStripMenuItem.Text = "&Game";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.GameNewStripMenuItem_Click);
            // 
            // replaySameGameToolStripMenuItem
            // 
            this.replaySameGameToolStripMenuItem.Name = "replaySameGameToolStripMenuItem";
            this.replaySameGameToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.replaySameGameToolStripMenuItem.Text = "&Replay Same Game";
            this.replaySameGameToolStripMenuItem.Click += new System.EventHandler(this.ReplaySameGameToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1164, 726);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.statusControl1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "Form1";
            this.Text = "Spirit Island - Single Player";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.textPanel.ResumeLayout(false);
            this.textPanel.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label promptLabel;
		private IslandControl islandControl;
		private CardControl cardControl;
        private SpiritControl spiritControl;
		private System.Windows.Forms.SplitContainer splitContainer;
		private System.Windows.Forms.Splitter textIslandSplitter;
		private System.Windows.Forms.Panel textPanel;
		private System.Windows.Forms.Splitter islandSpiritSplitter;
		private StatusControl statusControl1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem replaySameGameToolStripMenuItem;
	}
}

