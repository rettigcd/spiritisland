using SpiritIsland.Basegame;
using SpiritIsland.Core;
using SpiritIsland.SinglePlayer;
using System;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class ConfigureGame : Form {
		public ConfigureGame() {
			InitializeComponent();
		}

		void ConfigureGame_Load( object sender, EventArgs e ) {
			var spirits = new Type[] {
				typeof(RiverSurges),
				typeof(LightningsSwiftStrike),
				typeof(Shadows),
				typeof(VitalStrength),
				typeof(ThunderSpeaker),
			};
			spiritListBox.Items.Add("[Random]");
			foreach(var spirit in spirits) {
				spiritListBox.Items.Add(spirit);
			}
			spiritListBox.SelectedIndex = 0;

			// boards
			boardListBox.Items.Add( "[Random]" );
			boardListBox.Items.Add( "A" );
			boardListBox.Items.Add( "B" );
			boardListBox.Items.Add( "C" );
			boardListBox.Items.Add( "D" );
			boardListBox.SelectedIndex = 0;

			CheckOkStatus(null,null);
		}

		private void CheckOkStatus( object sender, EventArgs e ) {
			okButton.Enabled = true;
		}

		private void OkButton_Click( object sender, EventArgs e ) {
			Type spiritType = (spiritListBox.SelectedIndex == 0)
				? spiritListBox.Items[1 + (int)((DateTime.Now.Ticks/4) % 4)] as Type
				: spiritListBox.SelectedItem as Type;
			Spirit spirit = (Spirit)Activator.CreateInstance( spiritType );

			string boardOption = (boardListBox.SelectedIndex == 0)
				? boardListBox.Items[ 1+(int)(DateTime.Now.Ticks%4) ] as string
				: boardListBox.SelectedItem as string;

			var board = boardOption switch {
				"A" => Board.BuildBoardA(),
				"B" => Board.BuildBoardB(),
				"C" => Board.BuildBoardC(),
				"D" => Board.BuildBoardD(),
				_ => null,
			};

			var gameState = new GameState(
					spirit
				) {
				Island = new Island( board )
			};
			var baseGameFearCards = new IFearCard[] {
				new AvoidTheDahan(),
				new BeliefTakesRoot(),
				new DahanEnheartened(),
				new DahanOnTheirGuard(),
				new DahanRaid(),
				new EmigrationAccelerates(),
				new FearOfTheUnseen(),
				new Isolation(),
				new OverseasTradeSeemSafer(),
				new Retreat(),
				new Scapegoats(),
				new SeekSafety(),
				new TallTalesOfSavagery(),
				new TradeSuffers(),
				new WaryOfTheInterior()
			};
			baseGameFearCards.Shuffle();
			foreach(var f in baseGameFearCards.Take(9))
				gameState.FearDeck.Push(f);

			gameState.BlightCard = ((int)DateTime.Now.Ticks)%1==0 
				? new DownwardSpiral()
				: new MemoryFadesToDust();

			this.Game = new SinglePlayerGame( gameState );
		}

		public SinglePlayerGame Game { get; private set; }

	}
}
