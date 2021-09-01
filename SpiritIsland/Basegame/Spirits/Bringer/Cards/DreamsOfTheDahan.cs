using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DreamsOfTheDahan {

		[SpiritCard("Dreams of the Dahan",0,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(2)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// if target land has town/city,
			bool doFear = ctx.PowerInvaders.Counts.HasAny(Invader.City,Invader.Town)
				&& await ctx.Self.UserSelectsFirstText("Select power option","1 fear/dahan up to 3","gather up to 2 dahan");

			if(doFear)
				// 1 fear for each dahan, to a maximum of 3 fear
				ctx.AddFear(Math.Min(3,ctx.DahanCount));
			else
				// gather up to 2 dahan
				await ctx.GatherUpToNDahan(2);

		}
	}

}
