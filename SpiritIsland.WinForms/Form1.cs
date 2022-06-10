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
		
		}

		IDecision currentDecision;

		void Action_NewWaitingDecision( IDecision decision ) {

			// Decision
			currentDecision = decision;
			this.promptLabel.Text = decision.Prompt;
			islandControl.Invalidate();
			NewDecision?.Invoke( decision );
			
			UpdateRewindMenu();
		}

		void UpdateRewindMenu() {
			int rounds = game.GameState.RoundNumber;
			var items = rewindMenuItem.DropDownItems;
			if(items.Count == rounds) return; // no change
			items.Clear();
			for(int i = rounds; 0 < i; --i)
				items.Add( new ToolStripMenuItem( "Round " + i, null, RewindClicked ) { Tag = i } );
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
			logForm.AppendLine($"=== Game: {gc.SpiritType.Name} : {gc.Board} : {gc.ShuffleNumber} ===");

			var providers = new List<IGameComponentProvider> {
				new Basegame.GameComponentProvider(),
				new BranchAndClaw.GameComponentProvider(),
				new PromoPack1.GameComponentProvider(),
				new JaggedEarth.GameComponentProvider(),
			};
			GameState gameState = gameConfiguration.BuildGame( providers );
			game = new SinglePlayerGame( gameState, false ) { LogExceptions = true };
			game.Spirit.Action.NewWaitingDecision += Action_NewWaitingDecision;
			gameState.NewLogEntry += GameState_NewLogEntry; // !!! this should probably come through the user portal/gateway, not directly off of the gamestate.

			this.islandControl.Init( game.GameState, this, gameConfiguration.Color );
			this.cardControl.Init( game.Spirit, this );
			this.statusControl1.Init( game.GameState, this );
			this.Text = "Spirit Island - Single Player Game #"+gameConfiguration.ShuffleNumber;

			// start the game
			this.game.Start();

		}

		void GameState_NewLogEntry( ILogEntry obj ) {
			logForm.AppendLine(obj.Msg());

			if(obj is GameOver wle)
				Action_NewWaitingDecision( new Select.TypedDecision<TextOption>(wle.Msg(), Array.Empty<TextOption>() ) ); // clear options
		}

		void ExitToolStripMenuItem_Click( object sender, EventArgs e ) {
			Close();
		}

		#region Rewind

		void RewindClicked(object x, EventArgs y ) {
			int targetRound = (int)((ToolStripMenuItem)x).Tag;

			// This block is not necessary, but if something is wrong with the Memento, a hard-reset might be nice.
			if(targetRound == 1) {
				ReplaySameGameToolStripMenuItem_Click(null,null);
				return;
			}

			this.game.UserPortal.GoBackToBeginningOfRound(targetRound);
		}

		void ReplaySameGameToolStripMenuItem_Click( object _, EventArgs _1 ) {
			if(gameConfiguration!=null)
				InitGameFromConfiguration();
			else
				MessageBox.Show("No game configured.");
		}

		#endregion

		void GameLogToolStripMenuItem_Click( object sender, EventArgs e ) {
			logForm.Show();
		}

	}

	public interface IHaveOptions {
		event Action<IDecision> NewDecision;
    }

}
