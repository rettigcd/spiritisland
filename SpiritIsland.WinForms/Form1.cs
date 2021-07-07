using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpiritIsland.Base;
using SpiritIsland.Core;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.WinForms {
	public partial class Form1 : Form {

		readonly SinglePlayerGame game;

		class MessageBoxLogger : ILogger {
			public void Log( string text ) {
				MessageBox.Show(text);
			}
		}

		public Form1() {
			InitializeComponent();

			//game = new SinglePlayerGame(
			//	new GameState( new RiverSurges() ){ 
			//		Island = new Island(Board.BuildBoardA())
			//	}
			//	,new MessageBoxLogger()
			//);

			game = new SinglePlayerGame(
				new GameState( new LightningsSwiftStrike() ){ 
					Island = new Island(Board.BuildBoardA())
				}
				,new MessageBoxLogger()
			);


			this.islandControl.InitBoard(game.GameState);
			this.cardControl.Init(game.Spirit);
			this.islandControl.SpaceClicked += Select;
			this.cardControl.CardSelected += Select;
		}

		private void Form1_Load( object sender, EventArgs e ) {
			ShowOptions();
			UpdateDisplay();
		}

		void UpdateDisplay() {
			this.islandControl.Invalidate();

			var gs = game.GameState;
			// invader deck
			string ravage = gs.InvaderDeck.Ravage?.Text ?? "-";
			string build = gs.InvaderDeck.Build?.Text ?? "-";
			this.invaderBoardLabel.Text = $"Ravage {ravage}  Build {build}";

			// card
			var spirit = game.Spirit;
			this.trackLabel.Text = $"Energy: ${spirit.Energy} (+{spirit.EnergyPerTurn}/turn)  Cards Plays: {game.Spirit.NumberOfCardsPlayablePerTurn}/turn";

			this.blightLabel.Text = $"Blight: {gs.blightOnCard} " 
				+ (gs.IsBlighted ? "BLIGHTED":"");
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

			this.cardControl.HighlightCards(decision.Options);

			this.elementLabel.Text = game.Spirit.PurchasedCards
				.SelectMany(c=>c.Elements)
				.GroupBy(x=>x)
				.Select(grp=>grp.Count()+"-"+grp.Key)
				.Join(", ");
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

		void Select(IOption option){
			this.game.Decision.Select(option);
			this.ShowOptions();
			UpdateDisplay();
		}

		void Btn_Click( object sender, EventArgs e ) {
			var btn = (Button)sender;
			this.Select((IOption)btn.Tag);
		}

	}

}
