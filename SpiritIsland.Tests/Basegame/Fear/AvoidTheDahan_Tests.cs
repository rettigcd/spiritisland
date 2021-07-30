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
			gameState.InitIsland();

			invaderCard = InvaderDeck.Level1Cards[0];
			exploringSpace = gameState.Island.Boards[0].Spaces.Where( invaderCard.Matches ).First();

			// Given: initial BoardA hase explore in all lands
			//   And: 4 fear / player
			gameState.AddFear( 4 );

			//   And: 2 dahan on space
			gameState.AddDahan( exploringSpace, 2 );

		}

		[Fact]
		public void NullFearCard_NormalExplore() {
			gameState.FearCard = new NullFearCard();
			Space[] explored = When_ApplyFearAndExplore();
			Assert.Equal( 2, explored.Length );
		}

		[Fact]
		public void Level1_NoExplore() {
			gameState.FearCard = new AvoidTheDahan();
			Space[] explored = When_ApplyFearAndExplore();
			// Then: "Invaders do not Explore into lands with at least 2 Dahan."
			Assert.Single( explored );
		}

		Space[] When_ApplyFearAndExplore() {
			gameState.ApplyFear();
			Space[] explored = gameState.Explore( invaderCard );
			return explored;
		}


	}

}
