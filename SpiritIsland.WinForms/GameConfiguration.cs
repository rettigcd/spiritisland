using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
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
