using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.BranchAndClaw {
	public class KeeperPresence : MyPresence {

		public Spirit keeper;

		public KeeperPresence( PresenceTrack energy, PresenceTrack cardPlays )
			:base( energy, cardPlays )
		{ }

		public override async Task PlaceFromBoard( Track from, Space to, GameState gs ) {
			await base.PlaceFromBoard( from, to, gs );
			if(gs.DahanIsOn(to) && keeper.SacredSites.Contains(to))
				await keeper.MakeDecisionsFor(gs).Push(to,int.MaxValue,TokenType.Dahan);
				
		}

	}

}
