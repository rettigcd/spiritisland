using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EntrancingApparitions {

		[MinorCard("Entrancing Apparitions",1,Speed.Fast,Element.Moon,Element.Air,Element.Water)]
		[FromPresence(1)]
		static public async Task Act(TargetSpaceCtx ctx){

			// defend 2
			ctx.Defend(2);

			// if no invaders are present, gather 2 explorers
			if(!ctx.HasInvaders)
				await ctx.GatherUpToNInvaders(2, Invader.Explorer);

		}

	}
}
