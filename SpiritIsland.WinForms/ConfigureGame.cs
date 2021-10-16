using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using SpiritIsland.PromoPack1;
using SpiritIsland.SinglePlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class ConfigureGameDialog : Form {
		public ConfigureGameDialog() {
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
				// Promo Pack 1
				spiritListBox.Items.Add( typeof( HeartOfTheWildfire ) );
				spiritListBox.Items.Add( typeof( SerpentSlumbering ) );
				// Jagged Earth
				spiritListBox.Items.Add( typeof( LureOfTheDeepWilderness ) );
			}
			spiritListBox.SelectedIndex = 0;
		}

		private void CheckOkStatus( object sender, EventArgs e ) {
			okButton.Enabled = true;
		}

		private void OkButton_Click( object sender, EventArgs e ) {
			var gameSettings = new GameConfiguration {
				GameNumber = new Random().Next( 0, 999_999_999 ),
				UseBranchAndClaw = branchAndClawCheckBox.Checked,
				SpiritType = SelectedSpiritType(),
				UsePowerProgression = powerProgressionCheckBox.Checked,
				Board = SelectedBoard()
			};
			gameSettings.Color = (colorListBox.SelectedIndex == 0)
				? GetColorForSpirit( gameSettings.SpiritType )
				: colorListBox.SelectedItem as string;
			GameConfiguration = gameSettings;

		}

		static string GetColorForSpirit( Type spiritType ) {
			string spiritName = ((Spirit)Activator.CreateInstance( spiritType )).Text;
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

			return (spiritListBox.SelectedIndex == 0)
				? spiritListBox.Items[1 + (int)(DateTime.Now.Ticks % numberOfSpirits)] as Type
				: spiritListBox.SelectedItem as Type;
		}

		public GameConfiguration GameConfiguration { get; private set; }

		private void CheckBox1_CheckedChanged( object sender, EventArgs e ) {
			Init_SpiritList();
		}
	}

	public class GameConfiguration {
		public int GameNumber;
		public bool UseBranchAndClaw;
		public Type SpiritType;
		public bool UsePowerProgression;
		public string Board;
		public string Color;

		public GameState BuildGame() {
			var gameSettings = this;
			Spirit spirit = (Spirit)Activator.CreateInstance( gameSettings.SpiritType );
			if(gameSettings.UsePowerProgression) {
				try {
					spirit.UsePowerProgression();
				} catch {
					MessageBox.Show( "Unable to use power progression for " + spirit.Text );
				}
			}

			Board board = gameSettings.Board switch {
				"A" => SpiritIsland.Board.BuildBoardA(),
				"B" => SpiritIsland.Board.BuildBoardB(),
				"C" => SpiritIsland.Board.BuildBoardC(),
				"D" => SpiritIsland.Board.BuildBoardD(),
				_ => null,
			};

			var majorCards = PowerCard.GetMajors( typeof( AcceleratedRot ) ).ToList();
			var minorCards = PowerCard.GetMinors( typeof( AcceleratedRot ) ).ToList();

			if(gameSettings.UseBranchAndClaw) {
				majorCards.AddRange( PowerCard.GetMajors( typeof( CastDownIntoTheBrinyDeep ) ) );
				minorCards.AddRange( PowerCard.GetMinors( typeof( CastDownIntoTheBrinyDeep ) ) );
			}

			// GameState
			var gameState = !gameSettings.UseBranchAndClaw
				? new GameState( spirit, board )
				: new GameState_BranchAndClaw( spirit, board );

			// Game # - Randomizers
			var randomizer = new Random( gameSettings.GameNumber );

			gameState.InvaderDeck = new InvaderDeck( randomizer );

			// Shuffle Major / Minor Cards
			gameState.MajorCards = new PowerCardDeck( majorCards.ToArray(), randomizer );
			gameState.MinorCards = new PowerCardDeck( minorCards.ToArray(), randomizer );

			// --- start FEAR ---
			List<IFearOptions> fearCards = new List<IFearOptions>();
			fearCards.AddRange( SpiritIsland.Basegame.FearCards.GetFearCards() );
			if(gameSettings.UseBranchAndClaw)
				fearCards.AddRange( SpiritIsland.BranchAndClaw.FearCards.GetFearCards() );

			// Shuffle Fear cards
			randomizer.Shuffle( fearCards );

			gameState.Fear.Deck.Clear();
			foreach(var f in fearCards.Take( 9 ))
				gameState.Fear.AddCard( f );
			// --- End FEAR

			gameState.BlightCard = (randomizer.Next(1) == 0)
				? new DownwardSpiral()
				: new MemoryFadesToDust();
			return gameState;
		}

	}

}
