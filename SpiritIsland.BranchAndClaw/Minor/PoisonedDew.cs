using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class PoisonedDew {

		[MinorCard( "Poisoned Dew", 1, Element.Fire, Element.Water, Element.Plant )]
		[Slow]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			int countToDestroy = ctx.IsOneOf(Terrain.Jungle,Terrain.Wetland)
				? int.MaxValue
				: 1;

			return ctx.Invaders.Destroy(countToDestroy,Invader.Explorer);
		}

	}

}
