using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FireAndFlood {

		[MajorCard( "Fire and Flood", 9, Speed.Slow, Element.Sun, Element.Fire, Element.Water )]
		[FromSacredSite( 1 )]
//		[FromSacredSite( 2 )]  2 lands!!!
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// 4 damage in each target land  (range must be measured from same SS)
			return Task.CompletedTask;
		}

	}

}
