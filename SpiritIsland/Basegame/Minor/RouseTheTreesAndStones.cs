using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class RouseTheTreesAndStones {

		[MinorCard("Rouse the Trees and Stones",1,Speed.Slow,Element.Fire,Element.Earth,Element.Plant)]
		[FromSacredSite(1,Target.NoBlight)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			// 2 damage
			await ctx.DamageInvaders(2);
			// push 1 explorer
			await ctx.PushUpToNInvaders(1,Invader.Explorer);
		}

	}
}
