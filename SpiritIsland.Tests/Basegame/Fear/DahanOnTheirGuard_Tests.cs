using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
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
			gameState.AddFearCard( new DahanOnTheirGuard() );

			invaderCard = InvaderDeck.Level1Cards[0];
			ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
		}

		void Given_DahanAndTowns( int desiredDahan, int desiredTown ) {
			gameState.AdjustDahan( ravageSpace, desiredDahan - gameState.DahanCount( ravageSpace ) );
			Assert.Equal(desiredDahan,gameState.DahanCount(ravageSpace));

			gameState.Adjust( ravageSpace, InvaderSpecific.Town, desiredTown );
		}

		[Fact]
		public async Task NoFearCard_NormalRavage() {

			Given_DahanAndTowns( 2, 2 );

			// When: Doing Invader phase (fear+ragage)
			await gameState.ApplyFear();
			await gameState.Ravage(invaderCard );

			// Then: all dahan killed
			Assert.Equal( 0, gameState.DahanCount( ravageSpace ) );
			Assert.True( gameState.HasBlight( ravageSpace ) );
		}

		[Fact]
		public async Task Level1_DefendOnly1AndNotMorePerDahan() { // not more th
			Given_DahanAndTowns( 4, 4 );
			// 4 dahan should defend 4

			//   And: 4 fear / player
			gameState.AddFearDirect( new FearArgs{count=4} );

			// When: Doing Invader phase (fear+ragage)
			await gameState.ApplyFear();
			await gameState.Ravage(invaderCard);

			// Then: 0 dahan left
			gameState.DahanCount( ravageSpace ).ShouldBe(2);

			//   And: 2 towns
			gameState.InvadersOn( ravageSpace ).ToString().ShouldBe( "2T@2" );
			gameState.HasBlight( ravageSpace ).ShouldBe(true);
		}

	}

}
