using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SweepIntoTheSea {

		[MajorCard( "Sweep into the Sea", 4, Speed.Slow, Element.Sun, Element.Air, Element.Water )]
		[FromPresence( 2 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// push all explorere and town one land toward the nearest ocean
			// OR
			// if target land is costal, destory all explorers and town.
			// if you have 3 sun, 2 water: repeat on an adjacent land
			return Task.CompletedTask;
		}

	}
}
