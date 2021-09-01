using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
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
				typeof(Thunderspeaker),
				typeof(ASpreadOfRampantGreen),
				typeof(Bringer),
				typeof(Ocean),
				typeof(Keeper),
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


			CheckOkStatus( null,null);
		}

		private void CheckOkStatus( object sender, EventArgs e ) {
			okButton.Enabled = true;
		}

		private void OkButton_Click( object sender, EventArgs e ) {
			Spirit spirit = SelectedSpirit();
//			spirit.UsePowerProgression();
			Board board = SelectedBoard();

			Color = (colorListBox.SelectedIndex == 0)
				? GetColorForSpirit(spirit)
				: colorListBox.SelectedItem as string;

			var gameState = new GameState_BranchAndClaw( spirit, board ) {
				MajorCards = new PowerCardDeck( PowerCard.GetMajors() ),
				MinorCards = new PowerCardDeck( PowerCard.GetMinors() )
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
			gameState.Fear.Deck.Clear();
			foreach(var f in baseGameFearCards.Take( 9 ))
				gameState.Fear.AddCard( f );

			gameState.BlightCard = ((int)DateTime.Now.Ticks) % 1 == 0
				? new DownwardSpiral()
				: new MemoryFadesToDust();

			this.Game = new SinglePlayerGame( gameState );
		}

		static string GetColorForSpirit( Spirit spirit ) {
			return spirit.Text switch {
				RiverSurges.Name => "blue",
				LightningsSwiftStrike.Name => "yellow",
				VitalStrength.Name => "orange",
				Shadows.Name => "purple",
				Thunderspeaker.Name => "red",
				ASpreadOfRampantGreen.Name => "green",
				Bringer.Name => "pink",
				Ocean.Name => "dkblue",
				Keeper.Name => "greenorangeswirl",
				_ => "green"
			};
		}

		Board SelectedBoard() {
			string boardOption = (boardListBox.SelectedIndex == 0)
				? boardListBox.Items[1 + (int)(DateTime.Now.Ticks % 4)] as string
				: boardListBox.SelectedItem as string;

			var board = boardOption switch {
				"A" => Board.BuildBoardA(),
				"B" => Board.BuildBoardB(),
				"C" => Board.BuildBoardC(),
				"D" => Board.BuildBoardD(),
				_ => null,
			};
			return board;
		}

		Spirit SelectedSpirit() {
			Type spiritType = (spiritListBox.SelectedIndex == 0)
				? spiritListBox.Items[1 + (int)((DateTime.Now.Ticks / 4) % 4)] as Type
				: spiritListBox.SelectedItem as Type;
			Spirit spirit = (Spirit)Activator.CreateInstance( spiritType );
			return spirit;
		}

		public string Color { get; private set; }
		public SinglePlayerGame Game { get; private set; }

	}
}
