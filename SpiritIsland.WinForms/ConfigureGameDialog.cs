using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using SpiritIsland.FeatherAndFlame;
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
		new JaggedEarth.GameComponentProvider()
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
		spiritListBox.Items.Clear();
		spiritListBox.Items.Add( "[Random]" );
		foreach(var spirit in GameBuilder.SpiritNames)
			spiritListBox.Items.Add( spirit );
		spiritListBox.SelectedIndex = 0;
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

	PresenceTokenAppearance SelectAppearance( string spirit ) {
		return colorCheckBox.Checked
			? new PresenceTokenAppearance( HSL.FromRgb( colorDialog.Color ) )
			: GetColorForSpirit( spirit );
	}

	AdversaryConfig SelectAdversary() {
		string adversary = _adversaryListBox.SelectedItem.ToString();
		return adversary == "[None]" ? null
			: new AdversaryConfig( adversary, levelListBox.SelectedIndex );
	}

	// -.3f => .35

	static PresenceTokenAppearance GetColorForSpirit( string spiritName ) {
		return spiritName switch {
			Thunderspeaker.Name                  => new PresenceTokenAppearance( 0, .6f ),
			SharpFangs.Name                      => new PresenceTokenAppearance( 0, .8f, .35f ),
			VengeanceAsABurningPlague.Name       => new PresenceTokenAppearance( 15, .6f ),
			HeartOfTheWildfire.Name              => new PresenceTokenAppearance( 20, .8f ),
			VitalStrength.Name                   => new PresenceTokenAppearance( 22, .47f, .35f ),
			StonesUnyieldingDefiance.Name        => new PresenceTokenAppearance( 30, .16f ),
			LightningsSwiftStrike.Name           => new PresenceTokenAppearance( 55, .64f ),
			VolcanoLoomingHigh.Name              => new PresenceTokenAppearance( 56, 1.0f, .35f ),
			GrinningTricksterStirsUpTrouble.Name => new PresenceTokenAppearance( 58, .3f ),
			ASpreadOfRampantGreen.Name           => new PresenceTokenAppearance( 114, .65f, .45f ),
			LureOfTheDeepWilderness.Name         => new PresenceTokenAppearance( 125, .33f, .35f ),
			FracturedDaysSplitTheSky.Name        => new PresenceTokenAppearance( 160, .9f, .35f ),
			ShroudOfSilentMist.Name              => new PresenceTokenAppearance( 196, .3f, .65f ),
			Ocean.Name                           => new PresenceTokenAppearance( 200, .5f, .4f ),
			RiverSurges.Name                     => new PresenceTokenAppearance( 209, .5f, .4f ),
			DownpourDrenchesTheWorld.Name        => new PresenceTokenAppearance( 210, .7f, .35f ),
			FinderOfPathsUnseen.Name	         => new PresenceTokenAppearance( 218, .5f, .4f ),
			ShiftingMemoryOfAges.Name            => new PresenceTokenAppearance( 229, .35f ),
			StarlightSeeksItsForm.Name           => new PresenceTokenAppearance( 251, .78f ),
			Bringer.Name                         => new PresenceTokenAppearance( 300, .6f ),
			ManyMindsMoveAsOne.Name              => new PresenceTokenAppearance( 326, .35f ),
			SerpentSlumbering.Name               => new PresenceTokenAppearance( 330, .3f ),
			Shadows.Name                         => new PresenceTokenAppearance( 337, .3f, .35f ),
			Keeper.Name                          => new PresenceTokenAppearance( "greenorangeswirl" ),
			_                                    => new PresenceTokenAppearance( 0, 0 ),
		};
	}

	string SelectedSpiritName() {
		int numberOfSpirits = spiritListBox.Items.Count - 1;

		return spiritListBox.SelectedIndex == 0
			? (string)spiritListBox.Items[1 + (int)(DateTime.Now.Ticks % numberOfSpirits)]
			: (string)spiritListBox.SelectedItem;
	}

	string SelectedBoard() {
		return (boardListBox.SelectedIndex == 0)
			? boardListBox.Items[1 + (int)(DateTime.Now.Ticks % Board.AvailableBoards.Length)] as string
			: boardListBox.SelectedItem as string;
	}

	#endregion

	#region event hanlders

	void OkButton_Click( object sender, EventArgs e ) {
		string spirit = SelectedSpiritName();
		GameConfig = new GameConfigPlusToken {
			Spirits = new string[]{ spirit },
			Boards = new string[]{ SelectedBoard() },
			ShuffleNumber = SelectShuffleNumber(),
			Token = SelectAppearance( spirit ),
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
		levelListBox.Items.Clear();
		descriptionTextBox.Text = String.Empty;
		int index = _adversaryListBox.SelectedIndex;
		if( index != 0) {

			levelListBox.Enabled = true;
			string adversary = _adversaryListBox.SelectedItem as string;
			adjustments = GameBuilder.BuildAdversary( new AdversaryConfig(adversary,0) ).Adjustments;

			foreach(var level in adjustments)
				levelListBox.Items.Add( level.Title );

			levelListBox.SelectedIndex = 0;

		} else {
			levelListBox.Enabled = false;
		}
	}

	void levelListBox_SelectedIndexChanged( object sender, EventArgs e ) {
		descriptionTextBox.Text = adjustments[levelListBox.SelectedIndex].Description;
	}

	#endregion

	ScenarioLevel[] adjustments;

}

public class GameConfigPlusToken : GameConfiguration {
	// not a configuration, but when the game was played.
	public DateTime TimeStamp;
	public PresenceTokenAppearance Token;
}
