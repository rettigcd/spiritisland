using Shouldly;
using SpiritIsland.Basegame;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Fear {

	public class DahanOnTheirGuard_Tests {

		#region constructor

		public DahanOnTheirGuard_Tests() {
			spirit = new LightningsSwiftStrike();
			User = new VirtualUser( spirit );
			gameState = new GameState( spirit, Board.BuildBoardA() );
			gameState.DisableInvaderDeck();
			gameState.Initialize();
			gameState.Fear.Deck.Pop();
			gameState.Fear.AddCard( new DahanOnTheirGuard() );

			invaderCard = InvaderDeck.Level1Cards[0];
			ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
		}

		#endregion

		[Fact]
		public async Task NoFearCard_NormalRavage() {

			Given_DahanAndTowns( 2, 2 );

			// When: Doing Invader phase (fear+ragage)
			await gameState.Fear.Apply();
			await gameState.Ravage(invaderCard );

			// Then: all dahan killed
			Assert.Equal( 0, gameState.DahanGetCount( ravageSpace ) );
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
			User.AcknowledgesFearCard( "Dahan on their Guard:1:In each land, Defend 1 per Dahan." );

			// Then: 0 dahan left
			gameState.DahanGetCount( ravageSpace ).ShouldBe( 2 );

			//   And: 2 towns
			gameState.Assert_Invaders(ravageSpace, "2T@2" );
			gameState.HasBlight( ravageSpace ).ShouldBe( true );

		}

		void Given_DahanAndTowns( int desiredDahan, int desiredTown ) {
			gameState.DahanAdjust( ravageSpace, desiredDahan - gameState.DahanGetCount( ravageSpace ) );
			Assert.Equal(desiredDahan,gameState.DahanGetCount(ravageSpace));

			gameState.Tokens[ravageSpace].Adjust( Invader.Town.Default, desiredTown );
		}

		readonly GameState gameState;
		readonly InvaderCard invaderCard;
		readonly Space ravageSpace;
		readonly Spirit spirit;
		readonly VirtualUser User;

	}

}
