using System.Linq;
using Xunit;
using SpiritIsland.Basegame;
using System.Threading.Tasks;
using Shouldly;

namespace SpiritIsland.Tests.Basegame.Fear {
	
	public class BeliefTakesRoot_Tests {

		readonly GameState gameState;
		readonly InvaderCard invaderCard;
		readonly Space ravageSpace;

		public BeliefTakesRoot_Tests() {
			gameState = new GameState( new LightningsSwiftStrike(), Board.BuildBoardA() );
			gameState.DisableInvaderDeck();
			gameState.Initialize(); 
			gameState.FearDeck.Pop();
			gameState.AddFearCard( new BeliefTakesRoot() );

			invaderCard = InvaderDeck.Level1Cards[0];
			ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
		}

		void Given_DahanAndTownsInSpaceWithPresence(int desiredCount,int presenceCount) { 
			//   And: dahan & towns
			gameState.Dahan.Adjust( ravageSpace, desiredCount - gameState.Dahan.Count(ravageSpace) );
			gameState.Adjust( ravageSpace, InvaderSpecific.Town, desiredCount );

			//   And: Presence
			var spirit = gameState.Spirits[0];
			while(presenceCount-->0)
				spirit.Presence.PlaceOn( ravageSpace );
		}

		[Fact]
		public void NullFearCard_NormalRavage() {

			gameState.FearDeck.Pop();
			gameState.AddFearCard( new NullFearCard() );

			Given_DahanAndTownsInSpaceWithPresence(10,1);

			_ = When_AddFearApplyFearAndRavage();
			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", "Null Fear Card", "Null Fear Card" );

			// Then: all dahan killed
			gameState.Dahan.Count( ravageSpace ).ShouldBe(0);
			gameState.HasBlight( ravageSpace ).ShouldBe(true);
		}

		[Fact]
		public void Level1_NoBlightDahanLives() {
			Given_DahanAndTownsInSpaceWithPresence(1,1);

			_ =  When_AddFearApplyFearAndRavage();

			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", CardName, CardName );
			// Then: 1 dahan left
			Assert.Equal( 1, gameState.Dahan.Count( ravageSpace ) );

			//   And: 0 towns
			Assert.Equal( "", gameState.InvadersOn( ravageSpace ).ToString() );
			Assert.False( gameState.HasBlight( ravageSpace ) );

		}

		const string CardName = "Belief takes Root";

		[Fact]
		public void Level1_DefendNotMoreThan2() { // not more th
			Given_DahanAndTownsInSpaceWithPresence( 2, 5 );

			_ = When_AddFearApplyFearAndRavage();
			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", CardName, CardName );

			// Then: 1 dahan left
			Assert.Equal( 1, gameState.Dahan.Count( ravageSpace ) );

			//   And: 0 towns
			Assert.Equal( "1T@2", gameState.InvadersOn( ravageSpace ).ToString() );
			Assert.True( gameState.HasBlight( ravageSpace ) );
		}

		async Task When_AddFearApplyFearAndRavage() {
			gameState.AddFearDirect( new FearArgs{ count=4 } );
			await gameState.ApplyFear();
			await gameState.Ravage( invaderCard );
		}
	}
}
