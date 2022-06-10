
namespace SpiritIsland.WinForms {
	partial class ConfigureGameDialog {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing ) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.spiritListBox = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.boardListBox = new System.Windows.Forms.ListBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.colorListBox = new System.Windows.Forms.ListBox();
			this.powerProgressionCheckBox = new System.Windows.Forms.CheckBox();
			this.shuffleNumberTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// spiritListBox
			// 
			this.spiritListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left)));
			this.spiritListBox.DisplayMember = "Name";
			this.spiritListBox.FormattingEnabled = true;
			this.spiritListBox.ItemHeight = 15;
			this.spiritListBox.Location = new System.Drawing.Point(11, 27);
			this.spiritListBox.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.spiritListBox.Name = "spiritListBox";
			this.spiritListBox.Size = new System.Drawing.Size(282, 289);
			this.spiritListBox.TabIndex = 1;
			this.spiritListBox.SelectedIndexChanged += new System.EventHandler(this.CheckOkStatus);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(310, 10);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 15);
			this.label2.TabIndex = 2;
			this.label2.Text = "Select Board";
			// 
			// boardListBox
			// 
			this.boardListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left)));
			this.boardListBox.FormattingEnabled = true;
			this.boardListBox.ItemHeight = 15;
			this.boardListBox.Location = new System.Drawing.Point(310, 27);
			this.boardListBox.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.boardListBox.Name = "boardListBox";
			this.boardListBox.Size = new System.Drawing.Size(91, 289);
			this.boardListBox.TabIndex = 3;
			this.boardListBox.SelectedIndexChanged += new System.EventHandler(this.CheckOkStatus);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(8, 355);
			this.okButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(81, 22);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(106, 355);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(81, 22);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(411, 10);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(86, 15);
			this.label3.TabIndex = 6;
			this.label3.Text = "Presence Color";
			// 
			// colorListBox
			// 
			this.colorListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left)));
			this.colorListBox.FormattingEnabled = true;
			this.colorListBox.ItemHeight = 15;
			this.colorListBox.Location = new System.Drawing.Point(411, 28);
			this.colorListBox.Name = "colorListBox";
			this.colorListBox.Size = new System.Drawing.Size(100, 289);
			this.colorListBox.TabIndex = 7;
			// 
			// powerProgressionCheckBox
			// 
			this.powerProgressionCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.powerProgressionCheckBox.AutoSize = true;
			this.powerProgressionCheckBox.Location = new System.Drawing.Point(8, 318);
			this.powerProgressionCheckBox.Name = "powerProgressionCheckBox";
			this.powerProgressionCheckBox.Size = new System.Drawing.Size(146, 19);
			this.powerProgressionCheckBox.TabIndex = 9;
			this.powerProgressionCheckBox.Text = "Use Power Progression";
			this.powerProgressionCheckBox.UseVisualStyleBackColor = true;
			// 
			// shuffleNumberTextBox
			// 
			this.shuffleNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.shuffleNumberTextBox.Location = new System.Drawing.Point(411, 323);
			this.shuffleNumberTextBox.Name = "shuffleNumberTextBox";
			this.shuffleNumberTextBox.Size = new System.Drawing.Size(100, 23);
			this.shuffleNumberTextBox.TabIndex = 10;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(347, 326);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 15);
			this.label1.TabIndex = 11;
			this.label1.Text = "Shuffle #";
			// 
			// ConfigureGameDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(531, 390);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.shuffleNumberTextBox);
			this.Controls.Add(this.powerProgressionCheckBox);
			this.Controls.Add(this.colorListBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.boardListBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.spiritListBox);
			this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConfigureGameDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Configure New Game";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.ConfigureGame_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ListBox spiritListBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox boardListBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListBox colorListBox;
		private System.Windows.Forms.CheckBox powerProgressionCheckBox;
		private System.Windows.Forms.TextBox shuffleNumberTextBox;
		private System.Windows.Forms.Label label1;
	}
}