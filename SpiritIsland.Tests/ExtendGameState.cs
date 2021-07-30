using SpiritIsland.Core;
using System.Linq;

namespace SpiritIsland.Tests {
	static public class ExtendGameState {
		static public void DisableInvaderDeck(this GameState gs ) {
			var nullCard = new InvaderCard( Terrain.None );
			gs.InvaderDeck = new InvaderDeck( new byte[12].Select( _ => nullCard ).ToArray() );
		}
	}

	class NullFearCard : IFearCard {
		public void Level1( GameState gs ) {}

		public void Level2( GameState gs ) {}

		public void Level3( GameState gs ) {}
	}

}
