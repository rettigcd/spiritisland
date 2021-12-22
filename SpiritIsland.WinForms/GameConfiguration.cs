using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public class GameConfiguration {
		public int ShuffleNumber;
		public bool UseBranchAndClaw;
		public bool UseJaggedEarth;
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

			var majorCards = new List<PowerCard>();
			var minorCards = new List<PowerCard>();

			minorCards.AddRange( PowerCard.GetMinors( typeof( AcceleratedRot ) ) );
			majorCards.AddRange( PowerCard.GetMajors( typeof( AcceleratedRot ) ) );

			if(gameSettings.UseBranchAndClaw) {
				minorCards.AddRange( PowerCard.GetMinors( typeof( CastDownIntoTheBrinyDeep ) ) );
				majorCards.AddRange( PowerCard.GetMajors( typeof( CastDownIntoTheBrinyDeep ) ) );
			}
			if(gameSettings.UseJaggedEarth) {
				minorCards.AddRange( PowerCard.GetMinors( typeof( BatsScoutForRaidsByDarkness ) ) );
//				majorCards.AddRange( PowerCard.GetMajors( typeof( BatsScoutForRaidsByDarkness ) ) );
			}


			// GameState
			var gameState = new GameState( spirit, board );

			// Game # - Randomizers
			var randomizer = new Random( gameSettings.ShuffleNumber );

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

			// Enable Win / Loss Check
			gameState.ShouldCheckWinLoss = true; // !!! instead of this, load win/loss states into the check-list for real games

			return gameState;
		}

	}

}
