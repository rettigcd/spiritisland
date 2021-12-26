using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TippingPoint : BlightCardBase {

		public TippingPoint():base("Tipping Point", 5 ) { }

		protected override Task BlightAction( GameState gs ) 
			// Immediately, Each spirit
			=> GameCmd.EachSpirit( Cause.Blight,
				// destroys 3 presence
				SelfCmd.DestoryPresence(3,ActionType.BlightedIsland)
			).Execute( gs );


	}
}
