using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SmotheringInfestation {

		[MajorCard( "Smothering Infestation", 3, Speed.Slow, Element.Water, Element.Plant)]
		[FromPresence( 0 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// add 1 disease
			// if target land is J/W, 2 fear and 3 damage
			// if you have 2 water and 2 plant:  1 dmaage to each invader
			return Task.CompletedTask;
		}

	}
}
