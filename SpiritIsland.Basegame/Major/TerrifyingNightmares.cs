using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TerrifyingNightmares {

		[MajorCard("Terrifying Nightmares",4,Element.Moon,Element.Air)]
		[Fast]
		[FromPresence(2)]
		static public async Task Act(TargetSpaceCtx ctx){

			// 2 fear
			ctx.AddFear( 2 );

			// push up to 4 explorers or towns
			await ctx.PushUpTo(4, Invader.Explorer,Invader.Town);

			// if you have 4 moon, +4 fear
			if( await ctx.YouHave("4 moon") )
				ctx.AddFear(4);

		}

	}

}
