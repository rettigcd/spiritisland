using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

public partial class Form1 : Form, IHaveOptions {

	public Form1() {
		InitializeComponent();
		logForm = new LogForm();
	}

	readonly LogForm logForm;

	public event Action<IDecision> NewDecision;

	void Form1_Load( object sender, EventArgs e ) {
		this.islandControl.OptionSelected += Select;
	}

	void Select( IOption option ) {
		if(_currentDecision == null) return;

		// !! we could verify
		if(!_currentDecision.Options.Contains( option )) {
			MessageBox.Show(option.Text + " not found in option list");
			return;
		}

		_currentDecision = null;
		this.game.UserPortal.Choose( _currentDecision, option, false ); // If there is no decision to be made, just return
	
	}

	IDecision _currentDecision;

	void Action_NewWaitingDecision( IDecision decision ) {

		// Decision
		_currentDecision = decision;

		InitDecisionControls( decision );

		islandControl.Invalidate();

		// Trigger Next event....
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

	void InitDecisionControls( IDecision decision ) {
		// Create / populate controls
		_promptLabel.Text = decision.Prompt;

		using var calc = new FontSizeCalculator( this );
		ReleaseOldButtons();
		for(int i = 0; i < decision.Options.Length; ++i) {
			IOption option = decision.Options[i];
			var textSize = calc.CalcSize( option.Text );

			var btn = BuildOptionButton( option, new Size( (int)(textSize.Width + textSize.Height*2), (int)textSize.Height + 10 ) );
			_optionButtons.Add( btn );
		}

		PositionOptionControls();

	}

	// !! (call this bit when window is resized)
	void PositionOptionControls() {
		
		var bounds = this.islandControl.OptionBounds;
		var p = bounds.Location;
		p.Offset( islandControl.Location );
		_promptLabel.Location = p;
		int x = _promptLabel.Bounds.Left;
		int y = _promptLabel.Bounds.Bottom + 10;

		foreach(var btn in _optionButtons) {
			// Position it.
			btn.Location = new Point( x, y );
			y = btn.Bounds.Bottom + 5;
		}
	}

	Button BuildOptionButton( IOption option, Size sz ) {

		bool useMnemonic = (option.Text == "Done");

		var btn = new Button {
			Dock = DockStyle.None,
			Text = option.Text,
			Size = sz,
			Tag = option,
			UseMnemonic = useMnemonic,
		};
		if( useMnemonic ) btn.Text = "&" + btn.Text;

		btn.Click += Btn_Click;
		Controls.Add( btn );
		btn.BringToFront();
		return btn;
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
		foreach(var old in _optionButtons) {
			old.Click -= Btn_Click;
			Controls.Remove( old ); // textPanel
		}
		_optionButtons.Clear();
	}

	void Btn_Click( object sender, EventArgs e ) {
		var btn = (Button)sender;
		this.Select((IOption)btn.Tag);
	}

	#endregion

	readonly List<Button> _optionButtons = new();
	GameConfigPlusToken gameConfiguration;
	SinglePlayerGame game;

	void GameNewStripMenuItem_Click( object sender, EventArgs e ) {
		var gameConfigDialog = new ConfigureGameDialog();
		if(gameConfigDialog.ShowDialog() != DialogResult.OK) { return; }
		this.gameConfiguration = gameConfigDialog.GameConfig;
		InitGameFromConfiguration();
	}

	void InitGameFromConfiguration() {
		MySerializer.Add( gameConfiguration );

		logForm.Clear();

		var gc = gameConfiguration;

		logForm.AppendLine($"=== Game: {gc.Spirits[0]} : {gc.Boards[0]} : {gc.ShuffleNumber} : {gc.AdversarySummary} ===", LogLevel.Info ); // !!! show multiple boards/spirits

		GameState gameState = ConfigureGameDialog.GameBuilder.BuildGame( gc );
		game = new SinglePlayerGame( gameState, false ) { LogExceptions = true };

		game.Spirit.Gateway.NewWaitingDecision += Action_NewWaitingDecision;

		gameState.NewLogEntry += GameState_NewLogEntry; // !!! this should probably come through the user portal/gateway, not directly off of the gamestate.
		gameState.NewLogEntry += islandControl.GameState_NewLogEntry;

		this.islandControl.Init( game.GameState, this, gc.Token, gc.Adversary );
		this.Text = $"Spirit Island - Single Player Game #{gc.ShuffleNumber} - {gc.AdversarySummary}";

		// start the game
		this.game.Start();

	}

	void GameState_NewLogEntry( ILogEntry obj ) {
		logForm.AppendLine(obj.Msg(), obj.Level);

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

	void recentToolStripMenuItem_DropDownOpening( object sender, EventArgs e ) {
		recentToolStripMenuItem.DropDownItems.Clear();
		foreach(var x in MySerializer.GetRecent()) {
			var mi = new ToolStripMenuItem( $"{x.TimeStamp:MM/dd HH:mm}   { x.Spirits[0] } : {x.Boards[0]} : {x.ShuffleNumber}" ) { Tag = x };
			mi.Click += RecentGame_Clicked;
			recentToolStripMenuItem.DropDownItems.Add(mi);
		}

	}

	void RecentGame_Clicked( object sender, EventArgs _ ) {
		var tsmi = (ToolStripMenuItem)sender;
		this.gameConfiguration = (GameConfigPlusToken)tsmi.Tag;
		InitGameFromConfiguration();
	}

	void GameLogToolStripMenuItem_Click( object sender, EventArgs e ) {
		logForm.Show();
	}

	void ToggleDebugUIToolStripMenuItem_Click( object sender, EventArgs e ) {
		((ToolStripMenuItem)sender).Checked = islandControl.Debug = !islandControl.Debug;
	}

}

