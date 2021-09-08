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
			gameState.Fear.Deck.Pop();
			gameState.Fear.AddCard( new BeliefTakesRoot() );

			invaderCard = InvaderDeck.Level1Cards[0];
			ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
		}

		void Given_DahanAndTownsInSpaceWithPresence(int desiredCount,int presenceCount) { 
			//   And: dahan & towns
			gameState.DahanAdjust( ravageSpace, desiredCount - gameState.DahanGetCount(ravageSpace) );
			gameState.Tokens[ravageSpace].Adjust( Invader.Town.Default, desiredCount );

			//   And: Presence
			var spirit = gameState.Spirits[0];
			while(presenceCount-->0)
				spirit.Presence.PlaceOn( ravageSpace );
		}

		[Fact]
		public void NullFearCard_NormalRavage() {

			gameState.Fear.Deck.Pop();
			gameState.Fear.AddCard( new NullFearCard() );

			Given_DahanAndTownsInSpaceWithPresence(10,1);

			_ = When_AddFearApplyFearAndRavage();
			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", "Null Fear Card", "Null Fear Card" );

			// Then: all dahan killed
			gameState.DahanGetCount( ravageSpace ).ShouldBe(0);
			gameState.HasBlight( ravageSpace ).ShouldBe(true);
		}

		[Fact]
		public void Level1_NoBlightDahanLives() {
			Given_DahanAndTownsInSpaceWithPresence(1,1);

			_ =  When_AddFearApplyFearAndRavage();

			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", CardName, CardName );
			// Then: 1 dahan left
			Assert.Equal( 1, gameState.DahanGetCount( ravageSpace ) );

			//   And: 0 towns
			gameState.Assert_Invaders(ravageSpace,"");
			Assert.False( gameState.HasBlight( ravageSpace ) );

		}

		const string CardName = "Belief takes Root";

		[Fact]
		public void Level1_DefendNotMoreThan2() { // not more th
			Given_DahanAndTownsInSpaceWithPresence( 2, 5 );

			_ = When_AddFearApplyFearAndRavage();
			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", CardName, CardName );

			// Then: 1 dahan left
			Assert.Equal( 1, gameState.DahanGetCount( ravageSpace ) );

			//   And: 0 towns
			gameState.Assert_Invaders(ravageSpace, "1T@2" );
			Assert.True( gameState.HasBlight( ravageSpace ) );
		}

		async Task When_AddFearApplyFearAndRavage() {
			gameState.Fear.AddDirect( new FearArgs{ count=4 } );
			await gameState.Fear.Apply();
			await gameState.Ravage( invaderCard );
		}
	}

}
