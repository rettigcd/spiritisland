
namespace SpiritIsland {

	public class Invaders {

		// !! This wrapper class (around TokenCountDictionary) acts more like an Extension Method Class

		GameState gs;

		#region constructor

		public Invaders( GameState gs ) {
			this.gs = gs;
		}

		#endregion

		public InvaderGroup On( Space targetSpace, Cause cause ) {
			return new InvaderGroup( 
				gs.Tokens[targetSpace], 
				new DestroyInvaderStrategy( gs, gs.Fear.AddDirect, cause )
			);
		}

	}

}
