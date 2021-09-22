using System.Linq;
using Xunit;
using SpiritIsland.Basegame;
using System.Threading.Tasks;
using Shouldly;

namespace SpiritIsland.Tests.Basegame.Fear {
	
	public class BeliefTakesRoot_Tests : DecisionTests {

		readonly GameState gameState;
		readonly InvaderCard invaderCard;
		readonly Space ravageSpace;

		public BeliefTakesRoot_Tests():base(new LightningsSwiftStrike()) {
			gameState = new GameState( spirit, Board.BuildBoardA() );
			gameState.DisableInvaderDeck();
			gameState.Initialize(); 
			gameState.Fear.Deck.Pop();
			gameState.Fear.AddCard( new BeliefTakesRoot() );

			invaderCard = InvaderDeck.Level1Cards[0];
			ravageSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();
		}

		void Given_DahanAndTownsInSpaceWithPresence(int desiredCount,int presenceCount) { 
			// Add: dahan
			gameState.DahanAdjust( ravageSpace, desiredCount - gameState.DahanGetCount(ravageSpace) );
			// Add towns
			gameState.Tokens[ravageSpace].Adjust( Invader.Town.Default, desiredCount );

			//   And: Presence
			while(presenceCount-->0)
				spirit.Presence.PlaceOn( ravageSpace );
		}

		[Fact]
		public void NullFearCard_NormalRavage() {

			gameState.Fear.Deck.Pop();
			gameState.Fear.AddCard( new NullFearCard() );

			Given_DahanAndTownsInSpaceWithPresence(10,1);

			_ = When_AddFearApplyFearAndRavage();
			User.AcknowledgesFearCard("Null Fear Card:1:x");

			// Then: all dahan killed
			gameState.DahanGetCount( ravageSpace ).ShouldBe(0);
			gameState.HasBlight( ravageSpace ).ShouldBe(true);
		}

		[Fact]
		public void Level1_NoBlightDahanLives() {
			Given_DahanAndTownsInSpaceWithPresence(1,1);

			_ =  When_AddFearApplyFearAndRavage();

			User.AcknowledgesFearCard( FearCardAction );

			// Then: 1 dahan left
			Assert.Equal( 1, gameState.DahanGetCount( ravageSpace ) );

			//   And: 0 towns
			gameState.Assert_Invaders(ravageSpace,"");
			Assert.False( gameState.HasBlight( ravageSpace ) );

		}

		[Fact]
		public void Level1_DefendNotMoreThan2() { // not more th
			Given_DahanAndTownsInSpaceWithPresence( 2, 5 );

			_ = When_AddFearApplyFearAndRavage();

			User.AcknowledgesFearCard( FearCardAction );

			// Then: 1 dahan left
			Assert.Equal( 1, gameState.DahanGetCount( ravageSpace ) );

			//   And: 0 towns
			gameState.Assert_Invaders(ravageSpace, "1T@2" );
			Assert.True( gameState.HasBlight( ravageSpace ) );
		}

		const string FearCardAction = "Belief takes Root:1:Defend 2 in all lands with Presence.";

		async Task When_AddFearApplyFearAndRavage() {
			gameState.Fear.AddDirect( new FearArgs{ count=4 } );
			await gameState.Fear.Apply();
			await gameState.Ravage( invaderCard );
		}
	}

}
