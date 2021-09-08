using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class PoisonedDew {

		[MinorCard( "Poisoned Dew", 1, Speed.Slow, Element.Fire, Element.Water, Element.Plant )]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			int countToDestory = ctx.IsOneOf(Terrain.Jungle,Terrain.Wetland)
				? int.MaxValue
				: 1;
			return ctx.PowerInvaders.Destroy(countToDestory,Invader.Explorer);
		}

	}

}
