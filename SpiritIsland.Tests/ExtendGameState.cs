using Shouldly;
using SpiritIsland;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Tests {

	static public class ExtendGameState {

		static public void DisableInvaderDeck(this GameState gs ) {
			var nullCard = new InvaderCard( Terrain.None );
			gs.InvaderDeck = InvaderDeck.BuildTestDeck( new byte[12].Select( _ => nullCard ).ToArray() );
		}

		static public void Assert_Invaders( this GameState gameState, Space space, string expectedString ) {
			gameState.Tokens[ space ].InvaderSummary.ShouldBe( expectedString );
		}

	}

}
