using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallOfTheDeeps {

		[SpiritCard("Call of the Deeps",0,Speed.Fast,Element.Moon,Element.Air,Element.Water)]
		[FromPresence(0,Target.Costal)]
		static public Task Act(TargetSpaceCtx ctx ) {
			// Gather 1 explorer, if target land is the ocean, you may gather another explorer
			int count = ctx.Target.Terrain == Terrain.Ocean ? 2 : 1;
			return ctx.GatherUpToNInvaders(count,Invader.Explorer);
		}


	}
}
