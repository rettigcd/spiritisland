using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class LogForm : Form {
		public LogForm() {
			InitializeComponent();
		}

		public void Clear() {
			this.textBox.Text = "";
		}

		public void AppendLine(string s) {
			this.textBox.Text += "* " + s + "\r\n";
		}

		void LogForm_FormClosing( object sender, FormClosingEventArgs e ) {
			this.Hide();
			e.Cancel = true;    // Do not close the form.
		}

		private void CheckBox1_Click( object sender, System.EventArgs e ) {
			this.textBox.WordWrap = checkBox1.Checked;
		}
	}

}
