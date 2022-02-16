using SpiritIsland.Basegame;
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

			// !!! Call this from whereever spirit list is generated
			List<IGameComponentProvider> providers = GetProviders( gameSettings.UseBranchAndClaw );

			Spirit spirit = (Spirit)Activator.CreateInstance( gameSettings.SpiritType );
			if(gameSettings.UsePowerProgression) {
				try {
					spirit.UsePowerProgression();
				}
				catch {
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
			var fearCards = new List<IFearOptions>();
			var blightCards = new List<IBlightCard>();

			foreach(var provider in providers) {
				minorCards.AddRange( provider.MinorCards );
				majorCards.AddRange( provider.MajorCards );
				fearCards.AddRange( provider.FearCards );
				blightCards.AddRange( provider.BlightCards );
			}

			// GameState
			var gameState = new GameState( spirit, board );

			// Game # - Randomizers
			var randomizer = new Random( gameSettings.ShuffleNumber );

			gameState.InvaderDeck = new InvaderDeck( randomizer );

			// Shuffle
			gameState.MajorCards = new PowerCardDeck( majorCards.ToArray(), randomizer );
			gameState.MinorCards = new PowerCardDeck( minorCards.ToArray(), randomizer );
			randomizer.Shuffle( fearCards );
			randomizer.Shuffle( blightCards );

			gameState.Fear.Deck.Clear();
			foreach(var f in fearCards.Take( 9 ))
				gameState.Fear.AddCard( f );
			// --- End FEAR

			gameState.BlightCards = blightCards;
			gameState.BlightCard = blightCards[0];  
			blightCards.RemoveAt(0);

			// No Events
			var invaderCards = gameState.InvaderDeck.unrevealedCards;
			void InitBeastCommand(int stage) {
				for(int i = 0; i < invaderCards.Count; ++i) {
					if(invaderCards[i].InvaderStage != stage) continue;
					invaderCards[i] = new TriggerCommandBeasts( invaderCards[i] );
					break;
				}
			}
			InitBeastCommand(2);
			InitBeastCommand(3);

			// Enable Win / Loss Check
			gameState.ShouldCheckWinLoss = true; // !!! instead of this, load win/loss states into the check-list for real games

			return gameState;
		}

		public static List<IGameComponentProvider> GetProviders( bool useBranchAndClaw ) {
			List<IGameComponentProvider> providers = new List<IGameComponentProvider> {
				new GameComponentProvider()
			};
			if(useBranchAndClaw) {
				providers.Add( new BranchAndClaw.GameComponentProvider() );
				providers.Add( new PromoPack1.GameComponentProvider() );
				providers.Add( new JaggedEarth.GameComponentProvider() );
			}

			return providers;
		}
	}

}
