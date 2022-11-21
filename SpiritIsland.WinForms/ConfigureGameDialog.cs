using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using SpiritIsland.PromoPack1;
using System;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

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
				.SelectMany( p => p.Spirits )
				.Select( type => new SpiritBox { SpiritType = type } )
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
			colorListBox.Items.Add( "greenorangeswirl" );
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
			public Type SpiritType;
			public string Name => (string)SpiritType.GetField("Name").GetValue(null);
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
			var gameSettings = new GameConfiguration {
				SpiritType = SelectedSpiritType(),
				Board = SelectedBoard(),
				ShuffleNumber = ShuffleNumber(),
			};

			gameSettings.Color = (colorListBox.SelectedIndex == 0)
				? GetColorForSpirit( gameSettings.SpiritType )
				: colorListBox.SelectedItem as string;

			string adversary = _adversaryListBox.SelectedItem.ToString();
			gameSettings.AdversaryType = adversary.StartsWith("Brandenburg") ? typeof( BrandenburgPrussia ) 
				: adversary.StartsWith("England") ? typeof( England )
				: adversary.StartsWith( "Sweeden" ) ? typeof( Sweeden )
				: null;
			gameSettings.AdversaryLevel = string.IsNullOrEmpty( adversary ) ? 0 : (int)(adversary[^1] - '0');

			GameConfig = gameSettings;

		}

		int ShuffleNumber() {
			string txt = shuffleNumberTextBox.Text.Trim();
			return int.TryParse(txt, out int shuffleNumber) ? shuffleNumber
				: new Random().Next( 0, 999_999_999 );
		}

		static string GetColorForSpirit( Type spiritType ) {
			string spiritName = ((Spirit)Activator.CreateInstance( spiritType )).Text;		// !!! don't create a temp spirit and throw it away, just use the real spirit
			return spiritName switch {
				RiverSurges.Name           => "blue",
				LightningsSwiftStrike.Name => "yellow",
				VitalStrength.Name         => "orange",
				Shadows.Name               => "purple",
				Thunderspeaker.Name        => "red",
				ASpreadOfRampantGreen.Name => "green",
				Bringer.Name               => "pink",
				Ocean.Name                 => "dkblue",
				Keeper.Name                => "greenorangeswirl",
				SharpFangs.Name            => "red",
				HeartOfTheWildfire.Name    => "green",
				SerpentSlumbering.Name     => "orange",
				LureOfTheDeepWilderness.Name => "green",
				StonesUnyieldingDefiance.Name => "orange",
				_                          => "blue"
			};
		}

		string SelectedBoard() {
			return (boardListBox.SelectedIndex == 0)
				? boardListBox.Items[1 + (int)(DateTime.Now.Ticks % 4)] as string
				: boardListBox.SelectedItem as string;
		}

		Type SelectedSpiritType() {
			int numberOfSpirits = spiritListBox.Items.Count-1;

			var box = (spiritListBox.SelectedIndex == 0)
				? spiritListBox.Items[1 + (int)(DateTime.Now.Ticks % numberOfSpirits)] as SpiritBox
				: spiritListBox.SelectedItem as SpiritBox;
			return box.SpiritType;
		}

		public GameConfiguration GameConfig { get; private set; }

		private void CheckBox1_CheckedChanged( object sender, EventArgs e ) {
			Init_SpiritList();
		}

	}

}
