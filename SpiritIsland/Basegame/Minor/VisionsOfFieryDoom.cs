using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VisionsOfFieryDoom {
		[MinorCard("Visions of Fiery Doom",1, Speed.Fast,Element.Moon,Element.Fire)]
		[FromPresence(0)]
		static public async Task Act(TargetSpaceCtx ctx){
			var target = ctx.Target;

			// 1 fear
			bool hasBonus = ctx.Self.Elements[Element.Fire]>=2;
			ctx.AddFear(hasBonus?2:1);

			// Push 1 explorer/town
			await ctx.PushUpToNInvaders(target,1,Invader.Explorer,Invader.Town);
		}
	}
}
