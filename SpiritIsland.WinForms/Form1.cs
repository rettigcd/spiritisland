using SpiritIsland.Log;
using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

public partial class Form1 : Form, IHaveOptions {

	public Form1() {
		InitializeComponent();
		logForm = new LogForm();

		_init = new ArtworkInit( _promptLabel );
		_init.Init();
	}
	readonly ArtworkInit _init;

	readonly LogForm logForm;

	public event Action<IDecision> NewDecision;

	void Form1_Load( object sender, EventArgs e ) {
		_islandControl.OptionSelected += Select; // Allow Form to feed selection into IslandControl
		NewDecision += _islandControl.OptionProvider_OptionsChanged;
	}

	void Select( IOption option ) {
		if(_currentDecision == null) return;

		// !! we could verify
		if(!_currentDecision.Options.Contains( option )) {
			MessageBox.Show(option.Text + " not found in option list");
			return;
		}

		_currentDecision = null;
		_game.UserPortal.Choose( _currentDecision, option, false ); // If there is no decision to be made, just return
	
	}

	IDecision _currentDecision;

	void Action_NewWaitingDecision( IDecision decision ) {

		// Decision
		_currentDecision = decision;

		InitDecisionControls( decision );

		_islandControl.Invalidate();

		// Trigger Next event....
		NewDecision?.Invoke( decision );
		
		UpdateRewindMenu();
	}

	void UpdateRewindMenu() {
		int rounds = _game.GameState.RoundNumber;
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
		
		var bounds = this._islandControl.OptionBounds;
		var p = bounds.Location;
		p.Offset( _islandControl.Location );
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

	class FontSizeCalculator( Control control ) : IDisposable {
		Graphics graphics = control.CreateGraphics();
		readonly Font font = control.Font;

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

	readonly List<Button> _optionButtons = [];
	GameConfigPlusToken _gameConfiguration;
	SinglePlayerGame _game;

	void GameNewStripMenuItem_Click( object sender, EventArgs e ) {
		var gameConfigDialog = new ConfigureGameDialog();
		if(gameConfigDialog.ShowDialog() != DialogResult.OK) { return; }
		_gameConfiguration = gameConfigDialog.GameConfig;
		InitGameFromConfiguration();
	}

	void InitGameFromConfiguration() {
		MySerializer.Add( _gameConfiguration );

		logForm.Clear();

		var gc = _gameConfiguration;

		// Logging these directly to the Text box because the NewLogEntry event isn't hooked up yet.
		logForm.AppendLine($"=== Game: {gc.Spirits[0]} : {gc.Boards[0]} : {gc.ShuffleNumber} : {gc.AdversarySummary} ===", LogLevel.Info ); // !!! show multiple boards/spirits
		IAdversary adversary = ConfigureGameDialog.GameBuilder.BuildAdversary( gc.Adversary );
		var lossCond = adversary.LossCondition;
		if(lossCond != null ) logForm.AppendLine("Additional Loss Condition: "+lossCond.Description, LogLevel.Info );
		foreach(var advLevel in adversary.ActiveLevels)
			logForm.AppendLine($"Adversary {advLevel}:\r\n\t{advLevel.Description}", LogLevel.Info );

		GameState gameState = ConfigureGameDialog.GameBuilder.BuildGame( gc );

		_game = new SinglePlayerGame( gameState ) { 
			LogExceptions = true,
			EnablePreselects = true,
		};

		_game.Spirit.Portal.NewWaitingDecision += Action_NewWaitingDecision;

		gameState.NewLogEntry += GameState_NewLogEntry; // !!! this should probably come through the user portal/gateway, not directly off of the gamestate.
		gameState.NewLogEntry += _islandControl.GameState_NewLogEntry;

		_islandControl.Init( _game.GameState, gc.Adversary );

		Text = $"Spirit Island - Single Player Game #{gc.ShuffleNumber} - {gc.AdversarySummary}";

		// start the game
		_ = TryStartGame();

	}

	async Task TryStartGame() {
		try{
			ActionScope.Initialize(_game.GameState);
			await _game.StartAsync();
		} catch(Exception ex){
			System.IO.File.WriteAllText(System.IO.Path.Combine(AppDataFolder.GetRootPath(),"exception.txt"), ex.ToString() );
		}
	}

	void GameState_NewLogEntry( ILogEntry obj ) {
		logForm.AppendLine(obj.Msg(LogLevel.Info), obj.Level);

		if(obj is GameOverLogEntry wle)
			Action_NewWaitingDecision( new A.TypedDecision<TextOption>(wle.ToString(), Array.Empty<TextOption>() ) ); // clear options
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

		_game.UserPortal.GoBackToBeginningOfRound(targetRound);
	}

	void ReplaySameGameToolStripMenuItem_Click( object _, EventArgs _1 ) {
		if(_gameConfiguration!=null)
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
		_gameConfiguration = (GameConfigPlusToken)tsmi.Tag;
		_gameConfiguration.ConfigCommandBeasts(true); // !!! Hack - Command the beast is not being saved.  Save it instead of this.
		InitGameFromConfiguration();
	}

	void GameLogToolStripMenuItem_Click( object sender, EventArgs e ) {
		logForm.Show();
	}

	void ToggleDebugUIToolStripMenuItem_Click( object sender, EventArgs e ) {
		((ToolStripMenuItem)sender).Checked = _islandControl.Debug = !_islandControl.Debug;
	}

	void spaceTokensToolStripMenuItem_Click( object sender, EventArgs e ) {
		MessageBox.Show( _game.GameState.Tokens.ToVerboseString(), "Space Tokens" );
	}
}
