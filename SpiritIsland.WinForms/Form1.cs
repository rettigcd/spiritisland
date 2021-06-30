using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SpiritIsland.Base;
using SpiritIslandCmd;

namespace SpiritIsland.WinForms {
	public partial class Form1 : Form {

		readonly SinglePlayerGame game;

		public Form1() {
			InitializeComponent();

			game = new SinglePlayerGame(
				new GameState( new RiverSurges() ){ 
					Island = new Island(Board.BuildBoardA())
				}
			);

		}

		private void Form1_Load( object sender, EventArgs e ) {
			ShowOptions();
			UpdateBoardImage();
		}

		void UpdateBoardImage() {

		}

		void ShowOptions() {
			ReleaseOldButtons();

			var decision = game.Decision;
			this.promptLabel.Text = decision.Prompt;

			var options = decision.Options;
			for(int i=0;i<options.Length;++i)
				AddOptionButton( options[i], i );

		}

		void AddOptionButton( IOption option, int index ) {
			var btn = new System.Windows.Forms.Button {
				Dock = DockStyle.None,
				Location = new Point( 0, index * 50 + 75 ),
				Text = option.Text,
				Height = 45,
				Width = 400,
				Tag = option
			};
			btn.Click += Btn_Click;
			this.Controls.Add( btn );
			buttons.Add( btn );
		}

		void ReleaseOldButtons() {
			foreach(var old in buttons) {
				old.Click -= Btn_Click;
				this.Controls.Remove( old );
			}
			buttons.Clear();
		}

		readonly List<Button> buttons = new();

		private void Select(IOption option){
			this.game.Decision.Select(option);
			this.ShowOptions();
		}

		private void Btn_Click( object sender, EventArgs e ) {
			var btn = (Button)sender;
			this.Select((IOption)btn.Tag);
		}
	}

}
