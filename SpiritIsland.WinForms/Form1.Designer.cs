
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
			this.quitButton = new System.Windows.Forms.Button();
			this.promptLabel = new System.Windows.Forms.Label();
			this.islandControl = new SpiritIsland.WinForms.IslandControl();
			this.cardControl = new SpiritIsland.WinForms.CardControl();
			this.invaderBoardLabel = new System.Windows.Forms.Label();
			this.trackLabel = new System.Windows.Forms.Label();
			this.elementLabel = new System.Windows.Forms.Label();
			this.blightLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// quitButton
			// 
			this.quitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.quitButton.Location = new System.Drawing.Point(12, 1177);
			this.quitButton.Name = "quitButton";
			this.quitButton.Size = new System.Drawing.Size(150, 46);
			this.quitButton.TabIndex = 0;
			this.quitButton.Text = "Quit";
			this.quitButton.UseVisualStyleBackColor = true;
			// 
			// promptLabel
			// 
			this.promptLabel.AutoSize = true;
			this.promptLabel.Location = new System.Drawing.Point(12, 18);
			this.promptLabel.Name = "promptLabel";
			this.promptLabel.Size = new System.Drawing.Size(190, 32);
			this.promptLabel.TabIndex = 1;
			this.promptLabel.Text = "Decision Prompt";
			// 
			// islandControl
			// 
			this.islandControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.islandControl.Location = new System.Drawing.Point(530, 116);
			this.islandControl.Name = "islandControl";
			this.islandControl.Size = new System.Drawing.Size(1296, 674);
			this.islandControl.TabIndex = 2;
			this.islandControl.Text = "islandControl1";
			// 
			// cardControl
			// 
			this.cardControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cardControl.Location = new System.Drawing.Point(283, 592);
			this.cardControl.Name = "cardControl";
			this.cardControl.Size = new System.Drawing.Size(1574, 644);
			this.cardControl.TabIndex = 3;
			this.cardControl.Text = "cardControl1";
			// 
			// invaderBoardLabel
			// 
			this.invaderBoardLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.invaderBoardLabel.AutoSize = true;
			this.invaderBoardLabel.Location = new System.Drawing.Point(881, 18);
			this.invaderBoardLabel.Name = "invaderBoardLabel";
			this.invaderBoardLabel.Size = new System.Drawing.Size(168, 32);
			this.invaderBoardLabel.TabIndex = 4;
			this.invaderBoardLabel.Text = "Ravage / Build";
			// 
			// trackLabel
			// 
			this.trackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.trackLabel.AutoSize = true;
			this.trackLabel.Location = new System.Drawing.Point(1646, 18);
			this.trackLabel.Name = "trackLabel";
			this.trackLabel.Size = new System.Drawing.Size(180, 32);
			this.trackLabel.TabIndex = 5;
			this.trackLabel.Text = "Presence Tracks";
			// 
			// elementLabel
			// 
			this.elementLabel.AutoSize = true;
			this.elementLabel.Location = new System.Drawing.Point(881, 60);
			this.elementLabel.Name = "elementLabel";
			this.elementLabel.Size = new System.Drawing.Size(111, 32);
			this.elementLabel.TabIndex = 6;
			this.elementLabel.Text = "Elements";
			// 
			// blightLabel
			// 
			this.blightLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.blightLabel.AutoSize = true;
			this.blightLabel.Location = new System.Drawing.Point(1646, 60);
			this.blightLabel.Name = "blightLabel";
			this.blightLabel.Size = new System.Drawing.Size(76, 32);
			this.blightLabel.TabIndex = 7;
			this.blightLabel.Text = "Blight";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1869, 1235);
			this.Controls.Add(this.blightLabel);
			this.Controls.Add(this.elementLabel);
			this.Controls.Add(this.trackLabel);
			this.Controls.Add(this.invaderBoardLabel);
			this.Controls.Add(this.cardControl);
			this.Controls.Add(this.islandControl);
			this.Controls.Add(this.promptLabel);
			this.Controls.Add(this.quitButton);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button quitButton;
		private System.Windows.Forms.Label promptLabel;
		private IslandControl islandControl;
		private CardControl cardControl;
		private System.Windows.Forms.Label invaderBoardLabel;
		private System.Windows.Forms.Label trackLabel;
		private System.Windows.Forms.Label elementLabel;
		private System.Windows.Forms.Label blightLabel;
	}
}

