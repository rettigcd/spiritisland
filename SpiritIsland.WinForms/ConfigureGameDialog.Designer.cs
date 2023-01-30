
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
            this.components = new System.ComponentModel.Container();
            this.label2 = new System.Windows.Forms.Label();
            this.boardListBox = new System.Windows.Forms.ListBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.shuffleNumberTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._adversaryListBox = new System.Windows.Forms.ListBox();
            this.colorCheckBox = new System.Windows.Forms.CheckBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.levelListBox = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this._spiritListView = new System.Windows.Forms.ListView();
            this._spiritImageList = new System.Windows.Forms.ImageList(this.components);
            this.spiritLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(851, 347);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select Board";
            // 
            // boardListBox
            // 
            this.boardListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.boardListBox.FormattingEnabled = true;
            this.boardListBox.ItemHeight = 15;
            this.boardListBox.Location = new System.Drawing.Point(823, 363);
            this.boardListBox.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.boardListBox.Name = "boardListBox";
            this.boardListBox.Size = new System.Drawing.Size(100, 109);
            this.boardListBox.TabIndex = 3;
            this.boardListBox.SelectedIndexChanged += new System.EventHandler(this.CheckOkStatus);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(14, 522);
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
            this.cancelButton.Location = new System.Drawing.Point(112, 522);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 22);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // shuffleNumberTextBox
            // 
            this.shuffleNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.shuffleNumberTextBox.Location = new System.Drawing.Point(587, 521);
            this.shuffleNumberTextBox.Name = "shuffleNumberTextBox";
            this.shuffleNumberTextBox.Size = new System.Drawing.Size(100, 23);
            this.shuffleNumberTextBox.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(527, 524);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Shuffle #";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(540, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 15);
            this.label4.TabIndex = 12;
            this.label4.Text = "Adversary";
            // 
            // _adversaryListBox
            // 
            this._adversaryListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._adversaryListBox.FormattingEnabled = true;
            this._adversaryListBox.ItemHeight = 15;
            this._adversaryListBox.Location = new System.Drawing.Point(540, 28);
            this._adversaryListBox.Name = "_adversaryListBox";
            this._adversaryListBox.Size = new System.Drawing.Size(187, 289);
            this._adversaryListBox.TabIndex = 13;
            this._adversaryListBox.SelectedIndexChanged += new System.EventHandler(this.adversaryListBox_SelectedIndexChanged);
            // 
            // colorCheckBox
            // 
            this.colorCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.colorCheckBox.AutoSize = true;
            this.colorCheckBox.Location = new System.Drawing.Point(823, 477);
            this.colorCheckBox.Name = "colorCheckBox";
            this.colorCheckBox.Size = new System.Drawing.Size(100, 19);
            this.colorCheckBox.TabIndex = 14;
            this.colorCheckBox.Text = "Custom Color";
            this.colorCheckBox.UseVisualStyleBackColor = true;
            this.colorCheckBox.CheckedChanged += new System.EventHandler(this.colorCheckBox_CheckedChanged);
            // 
            // colorDialog
            // 
            this.colorDialog.AnyColor = true;
            this.colorDialog.Color = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.colorDialog.FullOpen = true;
            this.colorDialog.SolidColorOnly = true;
            // 
            // levelListBox
            // 
            this.levelListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.levelListBox.Enabled = false;
            this.levelListBox.FormattingEnabled = true;
            this.levelListBox.ItemHeight = 15;
            this.levelListBox.Location = new System.Drawing.Point(735, 29);
            this.levelListBox.Name = "levelListBox";
            this.levelListBox.Size = new System.Drawing.Size(187, 289);
            this.levelListBox.TabIndex = 15;
            this.levelListBox.SelectedIndexChanged += new System.EventHandler(this.levelListBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(735, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 15);
            this.label3.TabIndex = 16;
            this.label3.Text = "Level";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionTextBox.Location = new System.Drawing.Point(541, 359);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.ReadOnly = true;
            this.descriptionTextBox.Size = new System.Drawing.Size(257, 137);
            this.descriptionTextBox.TabIndex = 17;
            // 
            // _spiritListView
            // 
            this._spiritListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._spiritListView.LargeImageList = this._spiritImageList;
            this._spiritListView.Location = new System.Drawing.Point(11, 29);
            this._spiritListView.Name = "_spiritListView";
            this._spiritListView.Size = new System.Drawing.Size(520, 469);
            this._spiritListView.SmallImageList = this._spiritImageList;
            this._spiritListView.TabIndex = 18;
            this._spiritListView.TileSize = new System.Drawing.Size(64, 64);
            this._spiritListView.UseCompatibleStateImageBehavior = false;
            // 
            // _spiritImageList
            // 
            this._spiritImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this._spiritImageList.ImageSize = new System.Drawing.Size(80, 64);
            this._spiritImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // spiritLabel
            // 
            this.spiritLabel.AutoSize = true;
            this.spiritLabel.Location = new System.Drawing.Point(11, 9);
            this.spiritLabel.Name = "spiritLabel";
            this.spiritLabel.Size = new System.Drawing.Size(34, 15);
            this.spiritLabel.TabIndex = 19;
            this.spiritLabel.Text = "&Spirit";
            this.spiritLabel.Click += new System.EventHandler(this.spiritLabel_Click);
            // 
            // ConfigureGameDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(934, 578);
            this.Controls.Add(this.spiritLabel);
            this.Controls.Add(this._spiritListView);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.levelListBox);
            this.Controls.Add(this.colorCheckBox);
            this.Controls.Add(this._adversaryListBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.shuffleNumberTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.boardListBox);
            this.Controls.Add(this.label2);
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
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListBox boardListBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TextBox shuffleNumberTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ListBox _adversaryListBox;
		private System.Windows.Forms.CheckBox colorCheckBox;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.ListBox levelListBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox descriptionTextBox;
		private System.Windows.Forms.ListView _spiritListView;
		private System.Windows.Forms.ImageList _spiritImageList;
		private System.Windows.Forms.Label spiritLabel;
	}
}