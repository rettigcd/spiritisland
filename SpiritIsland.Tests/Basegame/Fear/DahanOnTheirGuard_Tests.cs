using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Basegame;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Fear {

	public class DahanOnTheirGuard_Tests {
		readonly GameState gameState;
		readonly InvaderCard invaderCard;
		readonly Space ravageSpace;

		public DahanOnTheirGuard_Tests() {
			gameState = new GameState( new LightningsSwiftStrike() ) {
				Island = new Island( Board.BuildBoardA() )
			};
			gameState.DisableInvaderDeck();
			gameState.InitIsland();
			gameState.FearCard = new DahanOnTheirGuard();

			invaderCard = InvaderDeck.Level1Cards[0];
			ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
		}

		void Given_DahanAndTowns( int desiredDahan, int desiredTown ) {
			gameState.AddDahan( ravageSpace, desiredDahan - gameState.GetDahanOnSpace( ravageSpace ) );
			Assert.Equal(desiredDahan,gameState.GetDahanOnSpace(ravageSpace));

			gameState.Adjust( ravageSpace, Invader.Town, desiredTown );
		}

		[Fact]
		public void NoFearCard_NormalRavage() {

			Given_DahanAndTowns( 2, 2 );

			// When: Doing Invader phase (fear+ragage)
			gameState.ApplyFear();
			gameState.Ravage( invaderCard );

			// Then: all dahan killed
			Assert.Equal( 0, gameState.GetDahanOnSpace( ravageSpace ) );
			Assert.True( gameState.HasBlight( ravageSpace ) );
		}

		[Fact]
		public void Level1_DefendOnly1AndNotMorePerDahan() { // not more th
			Given_DahanAndTowns( 4, 4 );

			//   And: 4 fear / player
			gameState.AddFear( 4 );

			// When: Doing Invader phase (fear+ragage)
			gameState.ApplyFear();
			gameState.Ravage( invaderCard );

			// Then: 0 dahan left
			Assert.Equal( 2, gameState.GetDahanOnSpace( ravageSpace ) );

			//   And: 2 towns
			Assert.Equal( "2T@2", gameState.InvadersOn( ravageSpace ).ToString() );
			Assert.True( gameState.HasBlight( ravageSpace ) );
		}


	}
}
