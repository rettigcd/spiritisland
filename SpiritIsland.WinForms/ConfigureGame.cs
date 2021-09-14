using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class ConfigureGame : Form {
		public ConfigureGame() {
			InitializeComponent();
		}

		void ConfigureGame_Load( object sender, EventArgs e ) {
			Init_SpiritList();

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


			CheckOkStatus( null, null );
		}

		private void Init_SpiritList() {
			spiritListBox.Items.Clear();
			spiritListBox.Items.Add( "[Random]" );
			// Base Game
			spiritListBox.Items.Add( typeof( RiverSurges ) );
			spiritListBox.Items.Add( typeof( LightningsSwiftStrike ) );
			spiritListBox.Items.Add( typeof( Shadows ) );
			spiritListBox.Items.Add( typeof( VitalStrength ) );
			spiritListBox.Items.Add( typeof( Thunderspeaker ) );
			spiritListBox.Items.Add( typeof( ASpreadOfRampantGreen ) );
			spiritListBox.Items.Add( typeof( Bringer ) );
			spiritListBox.Items.Add( typeof( Ocean ) );

			// Branch And Claw
			if(branchAndClawCheckBox.Checked) {
				spiritListBox.Items.Add( typeof( Keeper ) );
				spiritListBox.Items.Add( typeof( SharpFangs ) );
			}
			spiritListBox.SelectedIndex = 0;
		}

		private void CheckOkStatus( object sender, EventArgs e ) {
			okButton.Enabled = true;
		}

		private void OkButton_Click( object sender, EventArgs e ) {
			Spirit spirit = SelectedSpirit();
			if(powerProgressionCheckBox.Checked) {
				try {
					spirit.UsePowerProgression();
				} catch {
					MessageBox.Show( "Unable to use power progression for " + spirit.Text );
				}
			}
			Board board = SelectedBoard();

			Color = (colorListBox.SelectedIndex == 0)
				? GetColorForSpirit( spirit )
				: colorListBox.SelectedItem as string;

			var majorCards = PowerCard.GetMajors( typeof( AcceleratedRot ) ).ToList();
			var minorCards = PowerCard.GetMinors( typeof( AcceleratedRot ) ).ToList();

			if(branchAndClawCheckBox.Checked) {
				majorCards.AddRange( PowerCard.GetMajors( typeof( CastDownIntoTheBrinyDeep ) ) );
				minorCards.AddRange( PowerCard.GetMinors( typeof( CastDownIntoTheBrinyDeep ) ) );
			}

			var gameState = !branchAndClawCheckBox.Checked
				? new GameState( spirit, board )
				: new GameState_BranchAndClaw( spirit, board );

			gameState.MajorCards = new PowerCardDeck( majorCards.ToArray() );
			gameState.MinorCards = new PowerCardDeck( minorCards.ToArray() );

			List<IFearOptions> fearCards = new List<IFearOptions>();
			fearCards.AddRange( SpiritIsland.Basegame.FearCards.GetFearCards() );
			if(branchAndClawCheckBox.Checked)
				fearCards.AddRange( SpiritIsland.BranchAndClaw.FearCards.GetFearCards() );

			fearCards.Shuffle();

			gameState.Fear.Deck.Clear();
			foreach(var f in fearCards.Take( 9 ))
				gameState.Fear.AddCard( f );

//			gameState.Fear.AddDirect( new FearArgs { count = 4*8 } ); // !!!

			gameState.BlightCard = ((int)DateTime.Now.Ticks) % 1 == 0
				? new DownwardSpiral()
				: new MemoryFadesToDust();

			this.Game = new SinglePlayerGame( gameState );
		}

		static string GetColorForSpirit( Spirit spirit ) {
			return spirit.Text switch {
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
				_                          => "green"
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
			int numberOfSpirits = spiritListBox.Items.Count-1;

			Type spiritType = (spiritListBox.SelectedIndex == 0)
				? spiritListBox.Items[1 + (int)(DateTime.Now.Ticks % numberOfSpirits)] as Type
				: spiritListBox.SelectedItem as Type;
			Spirit spirit = (Spirit)Activator.CreateInstance( spiritType );
			return spirit;
		}

		public string Color { get; private set; }

		public SinglePlayerGame Game { get; private set; }

		private void CheckBox1_CheckedChanged( object sender, EventArgs e ) {
			Init_SpiritList();
		}
	}
}
