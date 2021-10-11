using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FieldsChokedWithGrowth {

		// push 1 town -OR- push 3 dahan
		[SpiritCard( "Fields Choked with Growth", 0, Element.Sun, Element.Water, Element.Plant )]
		[Slow]
		[FromPresence( 1 )]
		static public Task ActionAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption("Push 1 town", () => ctx.Push(1,Invader.Town),ctx.Tokens.Has(Invader.Town)),
				new ActionOption("Push 3 dahan", () => ctx.PushDahan(3),ctx.HasDahan)
			);

		}
	}
}
