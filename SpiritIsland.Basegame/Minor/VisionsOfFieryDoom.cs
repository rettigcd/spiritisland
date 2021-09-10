using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VisionsOfFieryDoom {

		[MinorCard("Visions of Fiery Doom",1, Speed.Fast,Element.Moon,Element.Fire)]
		[FromPresence(0)]
		static public async Task Act(TargetSpaceCtx ctx){
			// 1 fear
			ctx.AddFear( 1 );

			// Push 1 explorer/town
			await ctx.Push( 1, Invader.Explorer, Invader.Town );

			if(ctx.YouHave("2 fire"))
				ctx.AddFear( 1 );
		}

	}

}
