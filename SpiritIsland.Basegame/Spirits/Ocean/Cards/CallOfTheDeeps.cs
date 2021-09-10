using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallOfTheDeeps {

		[SpiritCard("Call of the Deeps",0,Speed.Fast,Element.Moon,Element.Air,Element.Water)]
		[FromPresence(0,Target.Coastal)]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			// Gather 1 explorer, if target land is the ocean, you may gather another explorer
			int count = ctx.Space.Terrain == Terrain.Ocean ? 2 : 1;
			return ctx.GatherUpTo(count,Invader.Explorer);

		}


	}
}
