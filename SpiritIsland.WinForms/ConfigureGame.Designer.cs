
namespace SpiritIsland.WinForms {
    partial class ConfigureGame {
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
            this.label1 = new System.Windows.Forms.Label();
            this.spiritListBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.boardListBox = new System.Windows.Forms.ListBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Spirit";
            // 
            // spiritListBox
            // 
            this.spiritListBox.FormattingEnabled = true;
            this.spiritListBox.ItemHeight = 32;
            this.spiritListBox.Location = new System.Drawing.Point(14, 74);
            this.spiritListBox.Name = "spiritListBox";
            this.spiritListBox.Size = new System.Drawing.Size(320, 260);
            this.spiritListBox.TabIndex = 1;
            this.spiritListBox.SelectedIndexChanged += new System.EventHandler(this.CheckOkStatus);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(398, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(147, 32);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select Board";
            // 
            // boardListBox
            // 
            this.boardListBox.FormattingEnabled = true;
            this.boardListBox.ItemHeight = 32;
            this.boardListBox.Location = new System.Drawing.Point(398, 74);
            this.boardListBox.Name = "boardListBox";
            this.boardListBox.Size = new System.Drawing.Size(314, 260);
            this.boardListBox.TabIndex = 3;
            this.boardListBox.SelectedIndexChanged += new System.EventHandler(this.CheckOkStatus);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(14, 363);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(150, 46);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(198, 363);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(150, 46);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ConfigureGame
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(763, 450);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.boardListBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.spiritListBox);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureGame";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Configure New Game";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ConfigureGame_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox spiritListBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox boardListBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}