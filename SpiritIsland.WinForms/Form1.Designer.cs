
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
            this.promptLabel = new System.Windows.Forms.Label();
            this.islandControl = new SpiritIsland.WinForms.IslandControl();
            this.cardControl = new SpiritIsland.WinForms.CardControl();
            this.invaderBoardLabel = new System.Windows.Forms.Label();
            this.trackLabel = new System.Windows.Forms.Label();
            this.spiritControl = new SpiritIsland.WinForms.SpiritControl();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.islandSpiritSplitter = new System.Windows.Forms.Splitter();
            this.textIslandSplitter = new System.Windows.Forms.Splitter();
            this.textPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.textPanel.SuspendLayout();
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
            this.islandControl.Location = new System.Drawing.Point(241, 0);
            this.islandControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.islandControl.Name = "islandControl";
            this.islandControl.Size = new System.Drawing.Size(964, 540);
            this.islandControl.TabIndex = 2;
            this.islandControl.Text = "islandControl1";
            // 
            // cardControl
            // 
            this.cardControl.BackColor = System.Drawing.Color.SaddleBrown;
            this.cardControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardControl.Location = new System.Drawing.Point(0, 0);
            this.cardControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cardControl.Name = "cardControl";
            this.cardControl.Size = new System.Drawing.Size(1514, 255);
            this.cardControl.TabIndex = 3;
            this.cardControl.Text = "cardControl1";
            // 
            // invaderBoardLabel
            // 
            this.invaderBoardLabel.AutoSize = true;
            this.invaderBoardLabel.Location = new System.Drawing.Point(11, 24);
            this.invaderBoardLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.invaderBoardLabel.Name = "invaderBoardLabel";
            this.invaderBoardLabel.Size = new System.Drawing.Size(83, 15);
            this.invaderBoardLabel.TabIndex = 4;
            this.invaderBoardLabel.Text = "Ravage / Build";
            // 
            // trackLabel
            // 
            this.trackLabel.AutoSize = true;
            this.trackLabel.Location = new System.Drawing.Point(11, 39);
            this.trackLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.trackLabel.Name = "trackLabel";
            this.trackLabel.Size = new System.Drawing.Size(89, 15);
            this.trackLabel.TabIndex = 5;
            this.trackLabel.Text = "Presence Tracks";
            // 
            // spiritControl
            // 
            this.spiritControl.BackColor = System.Drawing.Color.LightYellow;
            this.spiritControl.Dock = System.Windows.Forms.DockStyle.Right;
            this.spiritControl.Location = new System.Drawing.Point(1205, 0);
            this.spiritControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.spiritControl.Name = "spiritControl";
            this.spiritControl.Size = new System.Drawing.Size(309, 540);
            this.spiritControl.TabIndex = 8;
            this.spiritControl.Text = "spiritControl1";
            // 
            // logTextBox
            // 
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.logTextBox.Location = new System.Drawing.Point(0, 424);
            this.logTextBox.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(231, 116);
            this.logTextBox.TabIndex = 9;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.islandSpiritSplitter);
            this.splitContainer1.Panel1.Controls.Add(this.islandControl);
            this.splitContainer1.Panel1.Controls.Add(this.textIslandSplitter);
            this.splitContainer1.Panel1.Controls.Add(this.textPanel);
            this.splitContainer1.Panel1.Controls.Add(this.spiritControl);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cardControl);
            this.splitContainer1.Size = new System.Drawing.Size(1514, 799);
            this.splitContainer1.SplitterDistance = 540;
            this.splitContainer1.TabIndex = 10;
            // 
            // islandSpiritSplitter
            // 
            this.islandSpiritSplitter.Dock = System.Windows.Forms.DockStyle.Right;
            this.islandSpiritSplitter.Location = new System.Drawing.Point(1193, 0);
            this.islandSpiritSplitter.Name = "islandSpiritSplitter";
            this.islandSpiritSplitter.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.islandSpiritSplitter.Size = new System.Drawing.Size(12, 540);
            this.islandSpiritSplitter.TabIndex = 9;
            this.islandSpiritSplitter.TabStop = false;
            // 
            // textIslandSplitter
            // 
            this.textIslandSplitter.Location = new System.Drawing.Point(231, 0);
            this.textIslandSplitter.Name = "textIslandSplitter";
            this.textIslandSplitter.Size = new System.Drawing.Size(10, 540);
            this.textIslandSplitter.TabIndex = 2;
            this.textIslandSplitter.TabStop = false;
            // 
            // textPanel
            // 
            this.textPanel.Controls.Add(this.logTextBox);
            this.textPanel.Controls.Add(this.promptLabel);
            this.textPanel.Controls.Add(this.trackLabel);
            this.textPanel.Controls.Add(this.invaderBoardLabel);
            this.textPanel.Cursor = System.Windows.Forms.Cursors.Default;
            this.textPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.textPanel.Location = new System.Drawing.Point(0, 0);
            this.textPanel.Name = "textPanel";
            this.textPanel.Size = new System.Drawing.Size(231, 540);
            this.textPanel.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1514, 799);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.textPanel.ResumeLayout(false);
            this.textPanel.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label promptLabel;
		private IslandControl islandControl;
		private CardControl cardControl;
		private System.Windows.Forms.Label invaderBoardLabel;
		private System.Windows.Forms.Label trackLabel;
        private SpiritControl spiritControl;
        private System.Windows.Forms.TextBox logTextBox;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Splitter textIslandSplitter;
		private System.Windows.Forms.Panel textPanel;
		private System.Windows.Forms.Splitter islandSpiritSplitter;
	}
}

