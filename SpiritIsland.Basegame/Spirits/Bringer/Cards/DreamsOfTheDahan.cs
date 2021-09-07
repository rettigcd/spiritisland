using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DreamsOfTheDahan {

		[SpiritCard("Dreams of the Dahan",0,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(2)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption(
					"Gather up to 2 dahan",
					() => ctx.GatherUpToNDahan( 2 )
				),
				new ActionOption(
					"1 fear/dahan, max 3",
					() => ctx.AddFear(Math.Min(3,ctx.DahanCount)), 
					ctx.Tokens.HasAny( Invader.City, Invader.Town ) 
				)
			);

		}
	}

}
