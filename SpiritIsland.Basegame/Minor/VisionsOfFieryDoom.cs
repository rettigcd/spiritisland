using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VisionsOfFieryDoom {

		[MinorCard("Visions of Fiery Doom",1, Speed.Fast,Element.Moon,Element.Fire)]
		[FromPresence(0)]
		static public async Task Act(TargetSpaceCtx ctx){

			// 1 fear (+1 if 2 fire)
			ctx.AddFear( ctx.Self.Elements.Contains( "2 fire" ) ? 2 : 1 );

			// Push 1 explorer/town
			await ctx.PushUpToNTokens(1,Invader.Explorer,Invader.Town);
		}

	}

}
