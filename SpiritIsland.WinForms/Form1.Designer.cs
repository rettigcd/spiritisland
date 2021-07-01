
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
			this.promptLabel.Size = new System.Drawing.Size(78, 32);
			this.promptLabel.TabIndex = 1;
			this.promptLabel.Text = "label1";
			// 
			// islandControl
			// 
			this.islandControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.islandControl.Location = new System.Drawing.Point(527, 55);
			this.islandControl.Name = "islandControl";
			this.islandControl.Size = new System.Drawing.Size(1299, 1144);
			this.islandControl.TabIndex = 2;
			this.islandControl.Text = "islandControl1";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1869, 1235);
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
	}
}

