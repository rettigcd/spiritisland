using Shouldly;
using SpiritIsland.Basegame;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Fear {

	public class DahanOnTheirGuard_Tests {
		readonly GameState gameState;
		readonly InvaderCard invaderCard;
		readonly Space ravageSpace;

		public DahanOnTheirGuard_Tests() {
			gameState = new GameState( new LightningsSwiftStrike(), Board.BuildBoardA() );
			gameState.DisableInvaderDeck();
			gameState.Initialize();
			gameState.Fear.Deck.Pop();
			gameState.Fear.AddCard( new DahanOnTheirGuard() );

			invaderCard = InvaderDeck.Level1Cards[0];
			ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
		}

		void Given_DahanAndTowns( int desiredDahan, int desiredTown ) {
			gameState.Dahan.Adjust( ravageSpace, desiredDahan - gameState.Dahan.GetCount( ravageSpace ) );
			Assert.Equal(desiredDahan,gameState.Dahan.GetCount(ravageSpace));

			gameState.Invaders.Counts[ravageSpace].Add( Invader.Town, desiredTown );
		}

		[Fact]
		public async Task NoFearCard_NormalRavage() {

			Given_DahanAndTowns( 2, 2 );

			// When: Doing Invader phase (fear+ragage)
			await gameState.Fear.Apply();
			await gameState.Ravage(invaderCard );

			// Then: all dahan killed
			Assert.Equal( 0, gameState.Dahan.GetCount( ravageSpace ) );
			Assert.True( gameState.HasBlight( ravageSpace ) );
		}

		[Fact]
		public void Level1_DefendOnly1AndNotMorePerDahan() { // not more th
			Given_DahanAndTowns( 4, 4 );
			// 4 dahan should defend 4

			//   And: 4 fear / player
			gameState.Fear.AddDirect( new FearArgs{count=4} );

			// When: Doing Invader phase (fear+ragage)
			async Task DoIt() {
				await gameState.Fear.Apply();
				await gameState.Ravage(invaderCard);
			}
			_ = DoIt();
			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", "Dahan on their Guard", "Dahan on their Guard" );

			// Then: 0 dahan left
			gameState.Dahan.GetCount( ravageSpace ).ShouldBe( 2 );
			//   And: 2 towns
			gameState.Assert_Invaders(ravageSpace, "2T@2" );
			gameState.HasBlight( ravageSpace ).ShouldBe( true );

		}

	}

}
