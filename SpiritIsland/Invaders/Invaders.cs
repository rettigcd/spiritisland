
namespace SpiritIsland {

	public class Invaders {

		// !! This wrapper class (around SpaceInvaderCount) acts more like an Extension Method Class

		#region constructor

		public Invaders( GameState gs ) {
			this.gs = gs;
			this.gs.TimePassed += Heal;
		}

		#endregion

		public InvaderGroup On( Space targetSpace, Cause cause ) {
			var counts = gs.Tokens[targetSpace];
			return new InvaderGroup( targetSpace, counts ) {
				DestroyInvaderStrategy = new DestroyInvaderStrategy( gs.Fear.AddDirect, cause )
			};
		}

		#region private

		void Heal( GameState obj ) {
			foreach(var space in gs.Tokens.Keys)
				InvaderGroup.HealTokens( gs.Tokens[space] );
		}

		readonly GameState gs;

		#endregion

	}

}
