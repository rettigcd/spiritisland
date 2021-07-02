using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpiritIsland.Base;
using SpiritIsland.Core;
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

			this.islandControl.InitBoard(game.GameState);
			this.islandControl.SpaceClicked += Select;
			this.cardControl.CardSelected += Select;
		}

		private void Form1_Load( object sender, EventArgs e ) {
			ShowOptions();
			UpdateDisplay();
		}

		void UpdateDisplay() {
			this.islandControl.Invalidate();

			// invader deck
			string ravage = game.GameState.InvaderDeck.Ravage?.Text ?? "-";
			string build = game.GameState.InvaderDeck.Build?.Text ?? "-";
			this.invaderBoardLabel.Text = $"Ravage {ravage}  Build {build}";

			// card
			this.trackLabel.Text = $"Energy: ${game.Spirit.Energy} (+{game.Spirit.EnergyPerTurn}/turn)  Cards Plays: {game.Spirit.NumberOfCardsPlayablePerTurn}/turn";
		}

		void ShowOptions() {
			ReleaseOldButtons();

			var decision = game.Decision;
			this.promptLabel.Text = decision.Prompt;

			var options = decision.Options;
			for(int i=0;i<options.Length;++i)
				AddOptionButton( options[i], i );

			var spaceOptions = decision.Options.OfType<Space>().ToArray();
			this.islandControl.ActivateSpaces(spaceOptions);

			var cards = decision.Options.OfType<PowerCard>().ToArray();
			this.cardControl.ShowCards(cards);
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
			UpdateDisplay();
		}

		private void Btn_Click( object sender, EventArgs e ) {
			var btn = (Button)sender;
			this.Select((IOption)btn.Tag);
		}
	}

}
