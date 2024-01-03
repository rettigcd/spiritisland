using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

public partial class ConfigureGameDialog : Form {

	#region static

	static public readonly GameBuilder GameBuilder = new GameBuilder(
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new FeatherAndFlame.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider(),
		new NatureIncarnate.GameComponentProvider()
	);

	#endregion

	#region constructor / Init

	public ConfigureGameDialog() {
		InitializeComponent();
	}

	void ConfigureGame_Load( object sender, EventArgs e ) {
		Init_SpiritList();
		Init_BoardList();
		Init_AdversaryList();
		CheckOkStatus( null, null );
	}

	void Init_SpiritList() {
		int tileWidth = _spiritListView.Width / 2 - 20;
		_spiritListView.TileSize = new Size( tileWidth, _spiritImageList.ImageSize.Height * 9/8 );

		_spiritListView.Items.Clear();
		_spiritImageList.Images.Clear();

		foreach(var spiritName in GameBuilder.SpiritNames) {

			var listViewItem = new ListViewItem( spiritName, spiritName );
			_spiritListView.Items.Add( listViewItem );
			
			var img = ResourceImages.Singleton.LoadSpiritImage(spiritName);
			int index = _spiritImageList.Images.Count;
			_spiritImageList.Images.Add(img);
			_spiritImageList.Images.SetKeyName(index, spiritName );
		}

		// For unknown reason, we have to add 1 more image so that the last one doesn't get lost
		_spiritImageList.Images.Add( ResourceImages.Singleton.LoadSpiritImage( "Thunderspeaker" ) );
	}

	void Init_BoardList() {
		// boards
		boardListBox.Items.Add( "[Random]" );
		foreach(var availableBoard in Board.AvailableBoards)
			boardListBox.Items.Add( availableBoard );
		boardListBox.SelectedIndex = 0;
	}

	void Init_AdversaryList() {
		_adversaryListBox.Items.Add( "[None]" );
		foreach(var adversary in GameBuilder.AdversaryNames)
			_adversaryListBox.Items.Add( adversary );
		_adversaryListBox.SelectedIndex = 0;
	}

	#endregion

	public GameConfigPlusToken GameConfig { get; private set; }

	#region Set Game Config by Reading from Controls 

	int SelectShuffleNumber() {
		string txt = shuffleNumberTextBox.Text.Trim();
		return int.TryParse(txt, out int shuffleNumber) ? shuffleNumber
			: new Random().Next( 0, 999_999_999 );
	}

	AdversaryConfig SelectAdversary() {
		string adversary = _adversaryListBox.SelectedItem.ToString();
		return adversary == "[None]" ? null
			: new AdversaryConfig( adversary, _levelListBox.SelectedIndex );
	}

	string SelectedSpiritName() {
		int numberOfSpirits = _spiritListView.Items.Count;

		return _spiritListView.SelectedItems.Count == 1
			? _spiritListView.SelectedItems[0].Text
			: _spiritListView.Items[(int)(DateTime.Now.Ticks % numberOfSpirits)].Text;
	}

	string SelectedBoard() {
		return (boardListBox.SelectedIndex == 0)
			? boardListBox.Items[1 + (int)(DateTime.Now.Ticks % Board.AvailableBoards.Length)] as string
			: boardListBox.SelectedItem as string;
	}

	#endregion

	#region event hanlders

	void OkButton_Click( object sender, EventArgs e ) {
		string spiritName = SelectedSpiritName();
		GameConfig = new GameConfigPlusToken {
			Spirits = new string[]{ spiritName },
			Boards = new string[]{ SelectedBoard() },
			ShuffleNumber = SelectShuffleNumber(),
			Adversary = SelectAdversary(), 
			CommandTheBeasts = true,
		};
	}

	void CheckOkStatus( object sender, EventArgs e ) {
		okButton.Enabled = true;
	}

	void colorCheckBox_CheckedChanged( object sender, EventArgs e ) {
		if(this.colorCheckBox.Checked) {
			if( this.colorDialog.ShowDialog() == DialogResult.OK )
				this.colorCheckBox.BackColor = colorDialog.Color;
			else
				this.colorCheckBox.Checked = false;
		} else 
			this.colorCheckBox.BackColor = SystemColors.Control;
	}

	void adversaryListBox_SelectedIndexChanged( object sender, EventArgs e ) {
		_levelListBox.Items.Clear();
		descriptionTextBox.Text = String.Empty;
		int index = _adversaryListBox.SelectedIndex;
		if( index != 0)
			InitAdversaryLevels();
		else
			_levelListBox.Enabled = false;
	}

	void InitAdversaryLevels() {
		_levelListBox.Enabled = true;
		string adversary = _adversaryListBox.SelectedItem as string;
		_adjustments = GameBuilder.BuildAdversary( new AdversaryConfig( adversary, 0 ) ).Levels;

		foreach(var level in _adjustments)
			_levelListBox.Items.Add( level );

		_levelListBox.SelectedIndex = 0;
	}

	void levelListBox_SelectedIndexChanged( object sender, EventArgs e ) {
		descriptionTextBox.Text = _adjustments[_levelListBox.SelectedIndex].Description;
	}

	#endregion

	AdversaryLevel[] _adjustments;

	void spiritLabel_Click( object sender, EventArgs e ) {
		_spiritListView.View = _spiritListView.View == View.LargeIcon ? View.Tile : View.LargeIcon;
		spiritLabel.Text = $"Spirit - {_spiritListView.View}";
	}
}

public class GameConfigPlusToken : GameConfiguration {
	// not a configuration, but when the game was played.
	public DateTime TimeStamp;
}
