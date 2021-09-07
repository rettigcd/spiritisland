using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PyroclasticFlow {

		[MajorCard( "Pyroclastic Flow", 3, Speed.Fast, Element.Fire, Element.Air, Element.Earth )]
		[FromPresenceIn( 1, Terrain.Mountain )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// 2 damage. Destory all explorers
			// if target land is J/W, add 1 blight

			// if you have 2 fire, 3 air, 2 earth: +4 damage. Add 1 wilds
			return Task.CompletedTask;
		}

	}
}
