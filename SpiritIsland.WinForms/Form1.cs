using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class Form1 : Form, IHaveOptions {

		public Form1() {
			InitializeComponent();
			logForm = new LogForm();
		}

		readonly LogForm logForm;

		public event Action<IDecision> NewDecision;

		void Form1_Load( object sender, EventArgs e ) {

			this.NewDecision += UpdateButtons;
			this.islandControl.SpaceClicked += Select;
			this.islandControl.TokenClicked += Select;
			this.islandControl.SpaceTokenClicked += Select;
			this.islandControl.OptionSelected += Select;
			this.cardControl.CardSelected += Select;

		}

		void Select( IOption option ) {
			if(currentDecision == null) return;

			// !! we could verify
			if(!currentDecision.Options.Contains( option )) {
				MessageBox.Show(option.Text + " not found in option list");
				return;
			}

			currentDecision = null;
			this.game.UserPortal.Choose( option, false ); // If there is no decision to be made, just return
			
			if(this.game.WinLoseStatus == WinLoseStatus.Playing) return;

			this.Text = this.game.WinLoseStatus.ToString();
		}

		IDecision currentDecision;

		void Action_NewWaitingDecision( IDecision decision ) {
			currentDecision = decision;
			this.promptLabel.Text = decision.Prompt;
			islandControl.Invalidate();
			NewDecision?.Invoke( decision );
		}

		#region Buttons

		void UpdateButtons( IDecision decision ) {
			ReleaseOldButtons();
			using var calc = new FontSizeCalculator(this);
			var size = calc.CalcSize( this.promptLabel.Text );
			int x = (int)size.Width + 50;
			for(int i = 0; i < decision.Options.Length; ++i) {
				var option = decision.Options[i];
				size = calc.CalcSize( option.Text );
				var sz = new Size((int)size.Width+20,(int)size.Height+15);
				AddOptionButton( option, x, 1, sz );
				x += sz.Width+10;
			}
		}

		void AddOptionButton( IOption option, int x, int y, Size sz ) {
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
		}

		class FontSizeCalculator : IDisposable {
			Graphics graphics;
			readonly Font font;
			public FontSizeCalculator( Control control ) {
				this.graphics = control.CreateGraphics();
				this.font = control.Font;
			}

			public SizeF CalcSize( string s ) => graphics.MeasureString( s, font );

			public void Dispose() {
				if(graphics != null) {
					graphics.Dispose();
					graphics = null;
				}
			}
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
		GameConfiguration gameConfiguration;
		SinglePlayerGame game;

		void GameNewStripMenuItem_Click( object sender, EventArgs e ) {
			var gameConfigDialog = new ConfigureGameDialog();
			if(gameConfigDialog.ShowDialog() != DialogResult.OK) { return; }
			this.gameConfiguration = gameConfigDialog.GameConfig;
			InitGameFromConfiguration();
		}

		void InitGameFromConfiguration() {
			logForm.Clear();

			var gc = gameConfiguration;
			logForm.AppendLine($"=== Game: {gc.SpiritType.Name} - {gc.Board} - {gc.ShuffleNumber} ===");


			GameState gameState = gameConfiguration.BuildGame();
			game = new SinglePlayerGame( gameState, false ) { LogExceptions = true };
			game.Spirit.Action.NewWaitingDecision += Action_NewWaitingDecision;
			gameState.NewLogEntry += GameState_NewLogEntry;

			this.islandControl.Init( game.GameState, this, gameConfiguration.Color );
			this.cardControl.Init( game.Spirit, this );
			this.statusControl1.Init( game.GameState, this );
			this.Text = "Spirit Island - Single Player Game #"+gameConfiguration.ShuffleNumber;

			// start the game
			this.game.Start();

		}

		void GameState_NewLogEntry( ILogEntry obj ) {
			logForm.AppendLine(obj.Msg);
		}

		void ExitToolStripMenuItem_Click( object sender, EventArgs e ) {
			Close();
		}

		private void ReplaySameGameToolStripMenuItem_Click( object sender, EventArgs e ) {
			if(gameConfiguration!=null)
				InitGameFromConfiguration();
			else
				MessageBox.Show("No game configured.");
		}

		void ReplayRoundToolStripMenuItem_Click( object sender, EventArgs e ) {
			this.game.UserPortal.GoBackToBeginningOfRound();
		}

		private void GameLogToolStripMenuItem_Click( object sender, EventArgs e ) {
			logForm.Show();
		}
	}

	public interface IHaveOptions {
		event Action<IDecision> NewDecision;
    }

}
