
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
			_promptLabel = new System.Windows.Forms.Label();
			_islandControl = new IslandControl();
			textIslandSplitter = new System.Windows.Forms.Splitter();
			textPanel = new System.Windows.Forms.Panel();
			menuStrip1 = new System.Windows.Forms.MenuStrip();
			gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			rewindMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			gameLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toggleDebugUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			spaceTokensToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			modToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			addCardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			menuStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// _promptLabel
			// 
			_promptLabel.AutoSize = true;
			_promptLabel.Location = new System.Drawing.Point(7, 27);
			_promptLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			_promptLabel.Name = "_promptLabel";
			_promptLabel.Size = new System.Drawing.Size(95, 15);
			_promptLabel.TabIndex = 1;
			_promptLabel.Text = "Decision Prompt";
			// 
			// _islandControl
			// 
			_islandControl.BackColor = System.Drawing.Color.Aqua;
			_islandControl.Debug = false;
			_islandControl.Dock = System.Windows.Forms.DockStyle.Fill;
			_islandControl.Location = new System.Drawing.Point(0, 24);
			_islandControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			_islandControl.Name = "_islandControl";
			_islandControl.Size = new System.Drawing.Size(1164, 642);
			_islandControl.TabIndex = 2;
			_islandControl.Text = "islandControl1";
			// 
			// textIslandSplitter
			// 
			textIslandSplitter.Location = new System.Drawing.Point(0, 75);
			textIslandSplitter.Margin = new System.Windows.Forms.Padding(6);
			textIslandSplitter.Name = "textIslandSplitter";
			textIslandSplitter.Size = new System.Drawing.Size(19, 855);
			textIslandSplitter.TabIndex = 2;
			textIslandSplitter.TabStop = false;
			// 
			// textPanel
			// 
			textPanel.Dock = System.Windows.Forms.DockStyle.Top;
			textPanel.Location = new System.Drawing.Point(0, 0);
			textPanel.Margin = new System.Windows.Forms.Padding(6);
			textPanel.Name = "textPanel";
			textPanel.Size = new System.Drawing.Size(695, 75);
			textPanel.TabIndex = 1;
			// 
			// menuStrip1
			// 
			menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
			menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { gameToolStripMenuItem, viewToolStripMenuItem, modToolStripMenuItem });
			menuStrip1.Location = new System.Drawing.Point(0, 0);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Size = new System.Drawing.Size(1164, 24);
			menuStrip1.TabIndex = 12;
			menuStrip1.Text = "menuStrip1";
			// 
			// gameToolStripMenuItem
			// 
			gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newToolStripMenuItem, rewindMenuItem, recentToolStripMenuItem, exitToolStripMenuItem });
			gameToolStripMenuItem.Name = "gameToolStripMenuItem";
			gameToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
			gameToolStripMenuItem.Text = "&Game";
			// 
			// newToolStripMenuItem
			// 
			newToolStripMenuItem.Name = "newToolStripMenuItem";
			newToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			newToolStripMenuItem.Text = "&New";
			newToolStripMenuItem.Click += GameNewStripMenuItem_Click;
			// 
			// rewindMenuItem
			// 
			rewindMenuItem.Name = "rewindMenuItem";
			rewindMenuItem.Size = new System.Drawing.Size(128, 22);
			rewindMenuItem.Text = "Re&wind To";
			// 
			// recentToolStripMenuItem
			// 
			recentToolStripMenuItem.Name = "recentToolStripMenuItem";
			recentToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			recentToolStripMenuItem.Text = "&Recent";
			recentToolStripMenuItem.DropDownOpening += recentToolStripMenuItem_DropDownOpening;
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			exitToolStripMenuItem.Text = "E&xit";
			exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
			// 
			// viewToolStripMenuItem
			// 
			viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { gameLogToolStripMenuItem, toggleDebugUIToolStripMenuItem, spaceTokensToolStripMenuItem });
			viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			viewToolStripMenuItem.Text = "&View";
			// 
			// gameLogToolStripMenuItem
			// 
			gameLogToolStripMenuItem.Name = "gameLogToolStripMenuItem";
			gameLogToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			gameLogToolStripMenuItem.Text = "Game &Log";
			gameLogToolStripMenuItem.Click += GameLogToolStripMenuItem_Click;
			// 
			// toggleDebugUIToolStripMenuItem
			// 
			toggleDebugUIToolStripMenuItem.Name = "toggleDebugUIToolStripMenuItem";
			toggleDebugUIToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			toggleDebugUIToolStripMenuItem.Text = "Toggle Debug UI";
			toggleDebugUIToolStripMenuItem.Click += ToggleDebugUIToolStripMenuItem_Click;
			// 
			// spaceTokensToolStripMenuItem
			// 
			spaceTokensToolStripMenuItem.Name = "spaceTokensToolStripMenuItem";
			spaceTokensToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			spaceTokensToolStripMenuItem.Text = "Space";
			spaceTokensToolStripMenuItem.Click += spaceTokensToolStripMenuItem_Click;
			// 
			// modToolStripMenuItem
			// 
			modToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { addCardToolStripMenuItem });
			modToolStripMenuItem.Name = "modToolStripMenuItem";
			modToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			modToolStripMenuItem.Text = "&Mod";
			// 
			// addCardToolStripMenuItem
			// 
			addCardToolStripMenuItem.Name = "addCardToolStripMenuItem";
			addCardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			addCardToolStripMenuItem.Text = "Add &Card";
			addCardToolStripMenuItem.Click += addCardToolStripMenuItem_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(1164, 666);
			Controls.Add(_promptLabel);
			Controls.Add(_islandControl);
			Controls.Add(menuStrip1);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MainMenuStrip = menuStrip1;
			Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			Name = "Form1";
			Text = "Spirit Island - Single Player";
			Load += Form1_Load;
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Label _promptLabel;
		private IslandControl _islandControl;
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
		private System.Windows.Forms.ToolStripMenuItem toggleDebugUIToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem spaceTokensToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem modToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addCardToolStripMenuItem;
	}
}

