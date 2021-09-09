using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CastDownIntoTheBrinyDeep {

		[MajorCard( "Cast Down Into the Briny Deep", 9, Speed.Slow, Element.Sun, Element.Moon, Element.Water, Element.Earth )]
		[FromSacredSite( 1, Target.Coastal )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// 6 fear, destroy all invaders
			// if you have (2 sun, 2 moon, 4 water, 4 earth):
			// destory the board containing target land and everything on that board.  All destroyed blight is removed from the game instead of being returned to the blight card.
			return Task.CompletedTask;
		}

	}

}
