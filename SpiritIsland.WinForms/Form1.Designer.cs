
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
            this.textIslandSplitter = new System.Windows.Forms.Splitter();
            this.textPanel = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rewindMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Location = new System.Drawing.Point(7, 27);
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
            this.islandControl.Location = new System.Drawing.Point(0, 24);
            this.islandControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.islandControl.Name = "islandControl";
            this.islandControl.Size = new System.Drawing.Size(1164, 642);
            this.islandControl.TabIndex = 2;
            this.islandControl.Text = "islandControl1";
            // 
            // textIslandSplitter
            // 
            this.textIslandSplitter.Location = new System.Drawing.Point(0, 75);
            this.textIslandSplitter.Margin = new System.Windows.Forms.Padding(6);
            this.textIslandSplitter.Name = "textIslandSplitter";
            this.textIslandSplitter.Size = new System.Drawing.Size(19, 855);
            this.textIslandSplitter.TabIndex = 2;
            this.textIslandSplitter.TabStop = false;
            // 
            // textPanel
            // 
            this.textPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.textPanel.Location = new System.Drawing.Point(0, 0);
            this.textPanel.Margin = new System.Windows.Forms.Padding(6);
            this.textPanel.Name = "textPanel";
            this.textPanel.Size = new System.Drawing.Size(695, 75);
            this.textPanel.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameToolStripMenuItem,
            this.viewToolStripMenuItem});
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
            this.rewindMenuItem,
            this.recentToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.gameToolStripMenuItem.Text = "&Game";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.GameNewStripMenuItem_Click);
            // 
            // rewindMenuItem
            // 
            this.rewindMenuItem.Name = "rewindMenuItem";
            this.rewindMenuItem.Size = new System.Drawing.Size(128, 22);
            this.rewindMenuItem.Text = "Re&wind To";
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.recentToolStripMenuItem.Text = "&Recent";
            this.recentToolStripMenuItem.DropDownOpening += new System.EventHandler(this.recentToolStripMenuItem_DropDownOpening);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameLogToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // gameLogToolStripMenuItem
            // 
            this.gameLogToolStripMenuItem.Name = "gameLogToolStripMenuItem";
            this.gameLogToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.gameLogToolStripMenuItem.Text = "Game &Log";
            this.gameLogToolStripMenuItem.Click += new System.EventHandler(this.GameLogToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1164, 666);
            this.Controls.Add(this.promptLabel);
            this.Controls.Add(this.islandControl);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "Form1";
            this.Text = "Spirit Island - Single Player";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label promptLabel;
		private IslandControl islandControl;
		private System.Windows.Forms.Splitter textIslandSplitter;
		private System.Windows.Forms.Panel textPanel;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gameLogToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rewindMenuItem;
		private System.Windows.Forms.ToolStripMenuItem recentToolStripMenuItem;
	}
}

