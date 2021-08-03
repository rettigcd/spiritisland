using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Tests {
	static public class ExtendGameState {
		static public void DisableInvaderDeck(this GameState gs ) {
			var nullCard = new InvaderCard( Terrain.None );
			gs.InvaderDeck = new InvaderDeck( new byte[12].Select( _ => nullCard ).ToArray() );
		}
	}

	class NullFearCard : IFearCard {
		public Task Level1( GameState gs ) { return Task.CompletedTask;}

		public Task Level2( GameState gs ){
			throw new System.NotImplementedException();
		}

		public Task Level3( GameState gs ){
			throw new System.NotImplementedException();
		}
	}

}
