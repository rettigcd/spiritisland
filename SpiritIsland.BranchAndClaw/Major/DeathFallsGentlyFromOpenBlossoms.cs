using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class DeathFallsGentlyFromOpenBlossoms {

		[MajorCard("Death Falls Gently from Open Blossoms",4, Speed.Slow, Element.Moon,Element.Air,Element.Plant)]
		[FromPresence(3,Target.JungleOrSand)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			// 4 damage.  If any invaders remain, add 1 disease
			// if 3 air and 3 plant:  3 fear.  Add 1 disease to 2 adjacent lands with invaders.
			return Task.CompletedTask;
		}

	}

}
