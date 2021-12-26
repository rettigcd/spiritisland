using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class StrongEarthShattersSlowly : BlightCardBase {

		public StrongEarthShattersSlowly():base("Strong Earth Shatters Slowly",2) {}

		protected override Task BlightAction( GameState gs ) {
			// (Still healthy for now)
			// Immediately: Each player adds 1 blight (from this card) to a land adjacent to blight.
			return GameCmd.EachSpirit( Cause.Blight
				, SelfCmd.PickSpaceThenTakeAction("Adds 1 blight to a land adjacent to Blight."
					, Cmd.AddBlight
					, s => s.Adjacent.Any(a=>gs.Tokens[a].Blight.Any)
				)
			).Execute( gs );
		}

		protected override void Side2Depleted(  GameState gs ) {
			// If there is ever NO Blight here, draw a new Blight Card.
			gs.BlightCard = gs.BlightCards[0];
			gs.BlightCards.RemoveAt( 0 );
			// It comes into play already flipped
			gs.BlightCard.OnBlightDepleated( gs );
		}

	}

}
