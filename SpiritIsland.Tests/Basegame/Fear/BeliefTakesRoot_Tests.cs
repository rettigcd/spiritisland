using System.Linq;
using Xunit;
using SpiritIsland.Basegame;
using System.Threading.Tasks;

namespace SpiritIsland.Tests.Basegame.Fear {
	
	public class BeliefTakesRoot_Tests {

		readonly GameState gameState;
		readonly InvaderCard invaderCard;
		readonly Space ravageSpace;

		public BeliefTakesRoot_Tests() {
			gameState = new GameState( new LightningsSwiftStrike() ) {
				Island = new Island( Board.BuildBoardA() )
			};
			gameState.DisableInvaderDeck();
			gameState.FearDeck.Push( new BeliefTakesRoot() );
			gameState.InitIsland();

			invaderCard = InvaderDeck.Level1Cards[0];
			ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
		}

		void Given_DahanAndTownsInSpaceWithPresence(int desiredCount,int presenceCount) { 
			//   And: dahan & towns
			gameState.AddDahan( ravageSpace, desiredCount - gameState.GetDahanOnSpace(ravageSpace) );
			gameState.Adjust( ravageSpace, Invader.Town, desiredCount );

			//   And: Presence
			var spirit = gameState.Spirits[0];
			while(presenceCount-->0)
				spirit.Presence.Add( ravageSpace );
		}

		[Fact]
		public async Task NullFearCard_NormalRavage() {

			gameState.FearDeck.Push( new NullFearCard() );

			Given_DahanAndTownsInSpaceWithPresence(10,1);

			await When_AddFearApplyFearAndRavage();

			// Then: all dahan killed
			Assert.Equal( 0, gameState.GetDahanOnSpace( ravageSpace ) );
			Assert.True(gameState.HasBlight(ravageSpace));
		}

		[Fact]
		public async Task Level1_NoBlightDahanLives() {
			Given_DahanAndTownsInSpaceWithPresence(1,1);

			await When_AddFearApplyFearAndRavage();

			// Then: 1 dahan left
			Assert.Equal( 1, gameState.GetDahanOnSpace( ravageSpace ) );

			//   And: 0 towns
			Assert.Equal("", gameState.InvadersOn(ravageSpace).ToString() );
			Assert.False( gameState.HasBlight( ravageSpace ) );
		}

		[Fact]
		public async Task Level1_DefendNotMoreThan2() { // not more th
			Given_DahanAndTownsInSpaceWithPresence( 2, 5 );

			await When_AddFearApplyFearAndRavage();

			// Then: 1 dahan left
			Assert.Equal( 1, gameState.GetDahanOnSpace( ravageSpace ) );

			//   And: 0 towns
			Assert.Equal( "1T@2", gameState.InvadersOn( ravageSpace ).ToString() );
			Assert.True( gameState.HasBlight( ravageSpace ) );
		}

		async Task When_AddFearApplyFearAndRavage() {
			gameState.AddFear( 4 );
			await gameState.ApplyFear();
			gameState.Ravage( invaderCard );
		}
	}
}
