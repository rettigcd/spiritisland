using SpiritIsland;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Tests {

	static public class ExtendGameState {

		static public void DisableInvaderDeck(this GameState gs ) {
			var nullCard = new InvaderCard( Terrain.None );
			gs.InvaderDeck = new InvaderDeck( new byte[12].Select( _ => nullCard ).ToArray() );
		}

	}

}
