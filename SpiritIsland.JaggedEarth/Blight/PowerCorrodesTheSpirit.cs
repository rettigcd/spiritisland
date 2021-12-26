using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class PowerCorrodesTheSpirit : BlightCardBase {

		public PowerCorrodesTheSpirit():base("Power Corrodes the Spirit",4) {}

		protected override Task BlightAction( GameState gs ) {
			// Each Spirit
			return GameCmd.EachSpirit( Cause.Blight, DestroyPresenceForTooMuchPower )
				.Execute( gs );
		}

		static SelfAction DestroyPresenceForTooMuchPower => new SelfAction("Destroy 1 presence if spirit has 3 or more Power Cards in play or has a power card in play costing 4 or more Energy.",
			ctx => {
				// Each Invader Phase:
				// !!! this is supposed to be start of Invader Phase, not start of ravage
				ctx.GameState.PreRavaging.ForGame.Add( async (gs,args) => {
					// if they have 3 or more Power Cards in play, or have a Power Card in play costing 4 or more (printed) Energy.
					if( 3 <= ctx.Self.InPlay.Count || ctx.Self.InPlay.Any(c=>4<=c.Cost) )
						// Destorys 1 of their presence
						await ctx.Presence.DestoryOne(ActionType.BlightedIsland);
				});
			}
		);

	}

}
