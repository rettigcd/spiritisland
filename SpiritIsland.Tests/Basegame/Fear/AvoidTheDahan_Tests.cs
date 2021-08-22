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

			//   And: 2 dahan on space
			gameState.AdjustDahan( exploringSpace, 2 );

		}

		[Fact]
		public async Task NullFearCard_NormalExplore() {

			gameState.FearDeck.Pop();
			gameState.AddFearCard( new NullFearCard() );

			Space[] explored = await When_ApplyFearAndExplore();
			Assert.Equal( 2, explored.Length );
		}

		[Fact]
		async public Task Level1_NoExplore() {

			gameState.FearDeck.Pop();
			gameState.AddFearCard( new AvoidTheDahan() );

			Space[] explored = await When_ApplyFearAndExplore();
			// Then: "Invaders do not Explore into lands with at least 2 Dahan."
			Assert.Single( explored );
		}

		async Task<Space[]> When_ApplyFearAndExplore() {
			gameState.AddFearDirect( new FearArgs{ count=4 } );
			await gameState.ApplyFear();
			Space[] explored = gameState.Explore( invaderCard );
			return explored;
		}
		
	}

}
