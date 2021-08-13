using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SpiritIsland.SinglePlayer;

namespace SpiritIsland.WinForms {
	public partial class Form1 : Form, IHaveOptions {

		SinglePlayerGame game;

		public Form1() {
			InitializeComponent();
		}

		private void Form1_Load( object sender, EventArgs e ) {

			var config = new ConfigureGame();
			if(config.ShowDialog() != DialogResult.OK) { return; }
			this.game = config.Game;
			this.game.NewLogEntry += (msg) => this.logTextBox.AppendText(msg+"\r\n");

			this.islandControl.InitBoard( game.GameState );
			this.cardControl.Init( game.Spirit );
			this.spiritControl.Init( game.Spirit, config.Color, this );
			this.statusControl1.Init( game.GameState, this );
			this.islandControl.SpaceClicked += Select;
			this.cardControl.CardSelected += Select;
			this.spiritControl.OptionSelected += Select;

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

			// this.blightLabel.Text = $"Blight: {gs.blightOnCard} " + (gs.BlightCard.IslandIsBlighted ? "BLIGHTED":""); !!!
		}

		void ShowOptions() {
			ReleaseOldButtons();

			var decision = game.Decision;

			OptionsChanged?.Invoke(decision.Options);

			this.promptLabel.Text = decision.Prompt;

            IOption[] options = decision.Options;
			for(int i=0;i<options.Length;++i)
				AddOptionButton( options[i], i );

			this.islandControl.ActivateSpaces( decision.Options.OfType<Space>() );

			this.cardControl.HighlightCards(decision.Options);

		}

		void AddOptionButton( IOption option, int index ) {
			var btn = new System.Windows.Forms.Button {
				Dock = DockStyle.None,
				Location = new Point( 0, index * 50 + 150 ),
				Text = option.Text,
				Height = 45,
				Width = 250,
				Tag = option
			};
			btn.Click += Btn_Click;
			this.textPanel.Controls.Add( btn );
			buttons.Add( btn );
		}

		void ReleaseOldButtons() {
			foreach(var old in buttons) {
				old.Click -= Btn_Click;
				this.textPanel.Controls.Remove( old );
			}
			buttons.Clear();
		}

		readonly List<Button> buttons = new();

        public event Action<IOption[]> OptionsChanged;

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

	public interface IHaveOptions {
		event Action<IOption[]> OptionsChanged;
    }

}
