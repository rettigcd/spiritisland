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

			// Disable destroying presence
			gameState.DetermineAddBlightEffect = (gs,space) => new AddBlightEffect { Cascade=false,DestroyPresence=false };

			Given_DahanAndTowns( 2, 2 );

			// When: Doing Invader phase (fear+ragage)
			await gameState.Fear.Apply();
			await gameState.InvaderEngine.TestRavage(invaderCard );

			// Then: all dahan killed
			Assert.Equal( 0, gameState.DahanOn( ravageSpace ).Count );
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
				await gameState.InvaderEngine.TestRavage(invaderCard);
			}
			_ = DoIt();
			User.AcknowledgesFearCard( "Dahan on their Guard:1:In each land, Defend 1 per Dahan." );

			// Then: 0 dahan left
			gameState.DahanOn( ravageSpace ).Count.ShouldBe( 2 );

			//   And: 2 towns
			gameState.Assert_Invaders(ravageSpace, "2T@2" );
			gameState.HasBlight( ravageSpace ).ShouldBe( true );

		}

		void Given_DahanAndTowns( int desiredDahan, int desiredTown ) {
			gameState.DahanOn( ravageSpace ).Add( desiredDahan - gameState.DahanOn( ravageSpace ).Count );
			Assert.Equal(desiredDahan,gameState.DahanOn(ravageSpace).Count);

			gameState.Tokens[ravageSpace].Adjust( Invader.Town.Default, desiredTown );
		}

		readonly GameState gameState;
		readonly InvaderCard invaderCard;
		readonly Space ravageSpace;
		readonly Spirit spirit;
		readonly VirtualUser User;

	}

}
