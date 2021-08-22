using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TerrifyingNightmares {

		[MajorCard("Terrifying Nightmares",4,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(2)]
		static public async Task Act(TargetSpaceCtx ctx){

			// push up to 4 explorers or towns
			await ctx.PowerPushUpToNInvaders(4, Invader.Explorer,Invader.Town);

			// 2 fear
			ctx.AddFear(2);

			// if you have 4 moon, +4 fear
			if( ctx.Self.Elements.Contains("4 moon") )
				ctx.AddFear(4);

		}

	}

}
