using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DownwardSpiral : BlightCardBase {

		public DownwardSpiral():base("Downward Spiral",5) {}

		protected override async Task BlightAction( GameState gs ) {
			// Spirit destorys a presence.
			foreach(var spirit in gs.Spirits)
				await gs.Destroy1PresenceFromBlightCard( spirit, gs, Cause.Blight );
		}

	}


}
