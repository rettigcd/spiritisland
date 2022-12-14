using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using SpiritIsland.PromoPack1;
using System;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

public partial class ConfigureGameDialog : Form {

	#region static

	static public readonly IGameComponentProvider[] gameComponentProviders = new IGameComponentProvider[]{
		new Basegame.GameComponentProvider(),
		new BranchAndClaw.GameComponentProvider(),
		new PromoPack1.GameComponentProvider(),
		new JaggedEarth.GameComponentProvider()
	};

	static readonly SpiritBox[] spiritTypes;

	static ConfigureGameDialog() {
		spiritTypes = gameComponentProviders
			.SelectMany( p => p.SpiritNames )
			.Select( name => new SpiritBox { Name = name } )
			.OrderBy( t => t.Name )
			.ToArray();
	}

	#endregion

	public ConfigureGameDialog() {
		InitializeComponent();
	}

	void ConfigureGame_Load( object sender, EventArgs e ) {
		Init_SpiritList();

		// boards
		boardListBox.Items.Add( "[Random]" );
		foreach(var availableBoard in Board.AvailableBoards )
			boardListBox.Items.Add( availableBoard );
		boardListBox.SelectedIndex = 0;

		// color
		colorListBox.Items.Add( "[Automatic]" );
		colorListBox.Items.Add( "red" );
		colorListBox.Items.Add( "orange" );
		colorListBox.Items.Add( "yellow" );
		colorListBox.Items.Add( "green" );
		colorListBox.Items.Add( "blue" );
		colorListBox.Items.Add( "dkblue" );
		colorListBox.Items.Add( "purple" );
		colorListBox.Items.Add( "pink" );
		colorListBox.SelectedIndex = 0;

		// Adversaries
		_adversaryListBox.Items.Add("[None]");
		_adversaryListBox.Items.Add( "Brandenburg-Prussia 0" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Brandenburg-Prussia 1" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Brandenburg-Prussia 2" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Brandenburg-Prussia 3" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Brandenburg-Prussia 4" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Brandenburg-Prussia 5" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Brandenburg-Prussia 6" ); // !!! get from providers

		_adversaryListBox.Items.Add( "England 0" ); // !!! get from providers
		_adversaryListBox.Items.Add( "England 1" ); // !!! get from providers
		_adversaryListBox.Items.Add( "England 2" ); // !!! get from providers
		_adversaryListBox.Items.Add( "England 3" ); // !!! get from providers
		_adversaryListBox.Items.Add( "England 4" ); // !!! get from providers
		_adversaryListBox.Items.Add( "England 5" ); // !!! get from providers
		_adversaryListBox.Items.Add( "England 6" ); // !!! get from providers

		_adversaryListBox.Items.Add( "Sweeden 0" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Sweeden 1" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Sweeden 2" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Sweeden 3" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Sweeden 4" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Sweeden 5" ); // !!! get from providers
		_adversaryListBox.Items.Add( "Sweeden 6" ); // !!! get from providers
		_adversaryListBox.SelectedIndex = 0;
		CheckOkStatus( null, null );
	}

	class SpiritBox {
		public string Name { get; set; }
	}

	void Init_SpiritList() {

		spiritListBox.Items.Clear();
		spiritListBox.Items.Add( "[Random]" );

		foreach(var spirit in spiritTypes)
			spiritListBox.Items.Add( spirit );

		spiritListBox.SelectedIndex = 0;

	}

	void CheckOkStatus( object sender, EventArgs e ) {
		okButton.Enabled = true;
	}


void OkButton_Click( object sender, EventArgs e ) {
		var gameSettings = new GameConfigPlusToken {
			Spirit = SelectedSpiritName(),
			Board = SelectedBoard(),
			ShuffleNumber = ShuffleNumber(),
		};

		const float saturation = .7f; // standard
		gameSettings.Token = colorListBox.SelectedIndex == 0
			? GetColorForSpirit( gameSettings.Spirit )
			: (colorListBox.SelectedItem as string) switch {
				"red" => new PresenceTokenAppearance( 0f, saturation ),
				"orange" => new PresenceTokenAppearance( 30f, saturation ),
				"yellow" => new PresenceTokenAppearance( 60f, saturation, -.4f ),
				// missing 90
				"green" => new PresenceTokenAppearance( 120f, saturation, -.4f ),
				// missing 150
				"blue" => new PresenceTokenAppearance( 180f, saturation, -.3f ), // cyan
																				 // missing 210
				"dkblue" => new PresenceTokenAppearance( 240f, saturation ),
				"purple" => new PresenceTokenAppearance( 270f, saturation ),
				"pink" => new PresenceTokenAppearance( 300f, saturation ),
				"greenorangeswirl" => new PresenceTokenAppearance( 120, .4f, 0f, "greenorangeswirl" ),
				_ => new PresenceTokenAppearance( 200, .4f )
			};

		string adversary = _adversaryListBox.SelectedItem.ToString();
		gameSettings.Adversary = adversary == "[None]" ? null
			: new AdversaryConfig( adversary[..adversary.IndexOf( ' ' )], (int)(adversary[^1] - '0') );

		GameConfig = gameSettings;
	}

	int ShuffleNumber() {
		string txt = shuffleNumberTextBox.Text.Trim();
		return int.TryParse(txt, out int shuffleNumber) ? shuffleNumber
			: new Random().Next( 0, 999_999_999 );
	}

	static PresenceTokenAppearance GetColorForSpirit( string spiritName ) {
		return spiritName switch {
			Thunderspeaker.Name                  => new PresenceTokenAppearance( 0, .6f ),
			SharpFangs.Name                      => new PresenceTokenAppearance( 0, .8f, -.3f ),
			VengeanceAsABurningPlague.Name       => new PresenceTokenAppearance( 15, .6f ),
			HeartOfTheWildfire.Name              => new PresenceTokenAppearance( 20, .8f ),
			VitalStrength.Name                   => new PresenceTokenAppearance( 22, .47f, -.3f ),
			StonesUnyieldingDefiance.Name        => new PresenceTokenAppearance( 30, .16f ),
			LightningsSwiftStrike.Name           => new PresenceTokenAppearance( 55, .64f ),
			VolcanoLoomingHigh.Name              => new PresenceTokenAppearance( 56, 1.0f, -.3f ),
			GrinningTricksterStirsUpTrouble.Name => new PresenceTokenAppearance( 58, .3f ),
			ASpreadOfRampantGreen.Name           => new PresenceTokenAppearance( 114, .65f, -.1f ),
			LureOfTheDeepWilderness.Name         => new PresenceTokenAppearance( 125, .33f, -.3f ),
			FracturedDaysSplitTheSky.Name        => new PresenceTokenAppearance( 160, .9f, -.3f ),
			ShroudOfSilentMist.Name              => new PresenceTokenAppearance( 196, .3f, .3f ),
			Ocean.Name                           => new PresenceTokenAppearance( 200, .5f, -.2f ),
			RiverSurges.Name                     => new PresenceTokenAppearance( 209, .5f, -.2f ),
			DownpourDrenchesTheWorld.Name        => new PresenceTokenAppearance( 210, .7f, -.3f ),
			FinderOfPathsUnseen.Name	         => new PresenceTokenAppearance( 218, .5f, -.2f ),
			ShiftingMemoryOfAges.Name            => new PresenceTokenAppearance( 229, .35f ),
			StarlightSeeksItsForm.Name           => new PresenceTokenAppearance( 251, .78f ),
			Bringer.Name                         => new PresenceTokenAppearance( 300, .6f ),
			ManyMindsMoveAsOne.Name              => new PresenceTokenAppearance( 326, .35f ),
			SerpentSlumbering.Name               => new PresenceTokenAppearance( 330, .3f ),
			Shadows.Name                         => new PresenceTokenAppearance( 337, .3f, -.3f ),
			Keeper.Name                          => new PresenceTokenAppearance( "greenorangeswirl" ),
			_                                    => new PresenceTokenAppearance( 0, 0 ),
		};
	}

	string SelectedBoard() {
		return (boardListBox.SelectedIndex == 0)
			? boardListBox.Items[1 + (int)(DateTime.Now.Ticks % 4)] as string
			: boardListBox.SelectedItem as string;
	}

	string SelectedSpiritName() {
		int numberOfSpirits = spiritListBox.Items.Count-1;

		var box = (spiritListBox.SelectedIndex == 0)
			? spiritListBox.Items[1 + (int)(DateTime.Now.Ticks % numberOfSpirits)] as SpiritBox
			: spiritListBox.SelectedItem as SpiritBox;
		return box.Name;
	}

	public GameConfigPlusToken GameConfig { get; private set; }

	private void CheckBox1_CheckedChanged( object sender, EventArgs e ) {
		Init_SpiritList();
	}

}

public class GameConfigPlusToken : GameConfiguration {
	// not a configuration, but when the game was played.
	public DateTime TimeStamp;
	public PresenceTokenAppearance Token;
}
