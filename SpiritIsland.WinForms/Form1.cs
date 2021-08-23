using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.WinForms {
	public partial class Form1 : Form, IHaveOptions {

		public Form1() {
			InitializeComponent();
		}

		public event Action<IDecision> NewDecision;

		void Form1_Load( object sender, EventArgs e ) {

			var config = new ConfigureGame();
			if(config.ShowDialog() != DialogResult.OK) { return; }
			this.game = config.Game;
//			this.game.NewLogEntry += (msg) => this.logTextBox.AppendText(msg+"\r\n");

			var resourceImages = new ResourceImages();

			this.islandControl.Init( game.GameState, this, resourceImages, config.Color );
			this.cardControl.Init( game.Spirit, this );
			this.spiritControl.Init( game.Spirit, config.Color, this, resourceImages );
			this.statusControl1.Init( game.GameState, this );
			this.NewDecision += UpdateButtons;

			this.islandControl.SpaceClicked += Select;
			this.cardControl.CardSelected += Select;
			this.spiritControl.OptionSelected += Select;

			ShowOptions();
			islandControl.Invalidate();
		}


		void ShowOptions() {
			IDecision decision = game.DecisionProvider.GetCurrent();
			this.promptLabel.Text = decision.Prompt;
			NewDecision?.Invoke( decision );
		}

		void Select( IOption option ) {
			this.game.DecisionProvider.Choose( option );
			this.ShowOptions();
			islandControl.Invalidate();
		}

		#region Buttons

		void UpdateButtons( IDecision decision ) {
			ReleaseOldButtons();
			int x = CalcWidth(this.promptLabel.Text);
			for(int i = 0; i < decision.Options.Length; ++i) {
				x += AddOptionButton( decision.Options[i], x, 0 ).Width;
				x += 10;
			}
		}

		static int CalcWidth(string s ) => s.Length * 7 + 20;

		Size AddOptionButton( IOption option, int x, int y ) {
			Size sz = new Size( CalcWidth( option.Text ), 30);
			var btn = new System.Windows.Forms.Button {
				Dock = DockStyle.None,
				Location = new Point( x, y ),
				Text = option.Text,
				Size = sz,
				Tag = option
			};
			btn.Click += Btn_Click;
			this.textPanel.Controls.Add( btn );
			buttons.Add( btn );
			return sz;
		}

		void ReleaseOldButtons() {
			foreach(var old in buttons) {
				old.Click -= Btn_Click;
				this.textPanel.Controls.Remove( old );
			}
			buttons.Clear();
		}

		void Btn_Click( object sender, EventArgs e ) {
			var btn = (Button)sender;
			this.Select((IOption)btn.Tag);
		}

		#endregion

		readonly List<Button> buttons = new();
		SinglePlayerGame game;

	}

	public interface IHaveOptions {
		event Action<IDecision> NewDecision;
    }

}
