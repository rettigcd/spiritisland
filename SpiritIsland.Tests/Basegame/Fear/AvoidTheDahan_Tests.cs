using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SpiritIsland.Basegame;

namespace SpiritIsland.Tests.Basegame.Fear {

	public class AvoidTheDahan_Tests {

		readonly GameState gameState;
		readonly InvaderCard invaderCard;
		readonly Space exploringSpace;

		public AvoidTheDahan_Tests() {
			gameState = new GameState( new LightningsSwiftStrike() ) {
				Island = new Island( Board.BuildBoardA() )
			};
			gameState.Initialize();

			invaderCard = InvaderDeck.Level1Cards[0];
			exploringSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();

			//   And: 2 dahan on space
			gameState.AdjustDahan( exploringSpace, 2 );

		}

		[Fact]
		public void NullFearCard_NormalExplore() {

			gameState.FearDeck.Pop();
			gameState.AddFearCard( new NullFearCard() );

			_ = When_ApplyFearAndExplore();

			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", "Null Fear Card", "Null Fear Card" );


			Assert.Equal( 2, explored.Length );
		}

		[Fact]
		public void Level1_NoExplore() {

			gameState.FearDeck.Pop();
			gameState.AddFearCard( new AvoidTheDahan() );

			_ = When_ApplyFearAndExplore();
			gameState.Spirits[0].Action.AssertDecision( "Activating Fear", "Avoid the Dahan", "Avoid the Dahan" );

			// Then: "Invaders do not Explore into lands with at least 2 Dahan."
			Assert.Single( explored );
		}

		async Task When_ApplyFearAndExplore() {
			gameState.AddFearDirect( new FearArgs{ count=4 } );
			await gameState.ApplyFear();
			explored = gameState.Explore( invaderCard );
		}

		Space[] explored;

	}

}
