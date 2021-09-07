using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class StranglingFirevine {

		[MajorCard( "Strangling Firevine", 4, Speed.Slow, Element.Fire, Element.Plant )]
		[FromPresenceIn( 1, Terrain.Sand )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// destory all explorers.  Add 1 wilds.  Add 1 wilds in the originating Sands.  1 damage per wilds in / adjacent to target land.
			// if you have 2 fire, 3 plant: +1 damage per wilds in / adjacent to target land.
			return Task.CompletedTask;
		}


	}
}
