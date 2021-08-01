
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
            this.elementLabel = new System.Windows.Forms.Label();
            this.blightLabel = new System.Windows.Forms.Label();
            this.spiritControl = new SpiritIsland.WinForms.SpiritControl();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Location = new System.Drawing.Point(6, 8);
            this.promptLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(95, 15);
            this.promptLabel.TabIndex = 1;
            this.promptLabel.Text = "Decision Prompt";
            // 
            // islandControl
            // 
            this.islandControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.islandControl.Location = new System.Drawing.Point(285, 54);
            this.islandControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.islandControl.Name = "islandControl";
            this.islandControl.Size = new System.Drawing.Size(1206, 414);
            this.islandControl.TabIndex = 2;
            this.islandControl.Text = "islandControl1";
            // 
            // cardControl
            // 
            this.cardControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cardControl.Location = new System.Drawing.Point(0, 470);
            this.cardControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cardControl.Name = "cardControl";
            this.cardControl.Size = new System.Drawing.Size(1514, 247);
            this.cardControl.TabIndex = 3;
            this.cardControl.Text = "cardControl1";
            // 
            // invaderBoardLabel
            // 
            this.invaderBoardLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.invaderBoardLabel.AutoSize = true;
            this.invaderBoardLabel.Location = new System.Drawing.Point(982, 8);
            this.invaderBoardLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.invaderBoardLabel.Name = "invaderBoardLabel";
            this.invaderBoardLabel.Size = new System.Drawing.Size(83, 15);
            this.invaderBoardLabel.TabIndex = 4;
            this.invaderBoardLabel.Text = "Ravage / Build";
            // 
            // trackLabel
            // 
            this.trackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackLabel.AutoSize = true;
            this.trackLabel.Location = new System.Drawing.Point(1394, 8);
            this.trackLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.trackLabel.Name = "trackLabel";
            this.trackLabel.Size = new System.Drawing.Size(89, 15);
            this.trackLabel.TabIndex = 5;
            this.trackLabel.Text = "Presence Tracks";
            // 
            // elementLabel
            // 
            this.elementLabel.AutoSize = true;
            this.elementLabel.Location = new System.Drawing.Point(474, 28);
            this.elementLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.elementLabel.Name = "elementLabel";
            this.elementLabel.Size = new System.Drawing.Size(55, 15);
            this.elementLabel.TabIndex = 6;
            this.elementLabel.Text = "Elements";
            // 
            // blightLabel
            // 
            this.blightLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.blightLabel.AutoSize = true;
            this.blightLabel.Location = new System.Drawing.Point(1394, 28);
            this.blightLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.blightLabel.Name = "blightLabel";
            this.blightLabel.Size = new System.Drawing.Size(38, 15);
            this.blightLabel.TabIndex = 7;
            this.blightLabel.Text = "Blight";
            // 
            // spiritControl
            // 
            this.spiritControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.spiritControl.BackColor = System.Drawing.Color.LightYellow;
            this.spiritControl.Location = new System.Drawing.Point(1091, 92);
            this.spiritControl.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.spiritControl.Name = "spiritControl";
            this.spiritControl.Size = new System.Drawing.Size(417, 350);
            this.spiritControl.TabIndex = 8;
            this.spiritControl.Text = "spiritControl1";
            // 
            // logTextBox
            // 
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.logTextBox.Location = new System.Drawing.Point(0, 717);
            this.logTextBox.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(1514, 82);
            this.logTextBox.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1514, 799);
            this.Controls.Add(this.spiritControl);
            this.Controls.Add(this.blightLabel);
            this.Controls.Add(this.elementLabel);
            this.Controls.Add(this.trackLabel);
            this.Controls.Add(this.invaderBoardLabel);
            this.Controls.Add(this.promptLabel);
            this.Controls.Add(this.cardControl);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.islandControl);
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label promptLabel;
		private IslandControl islandControl;
		private CardControl cardControl;
		private System.Windows.Forms.Label invaderBoardLabel;
		private System.Windows.Forms.Label trackLabel;
		private System.Windows.Forms.Label elementLabel;
		private System.Windows.Forms.Label blightLabel;
        private SpiritControl spiritControl;
        private System.Windows.Forms.TextBox logTextBox;
    }
}

