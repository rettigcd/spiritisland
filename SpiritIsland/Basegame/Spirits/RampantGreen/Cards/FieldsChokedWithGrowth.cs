using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class FieldsChokedWithGrowth {

		// push 1 town -OR- push 3 dahan
		[SpiritCard( "Fields Choked with Growth", 0, Speed.Slow, Element.Sun, Element.Water, Element.Plant )]
		[FromPresence( 1 )]
		static public Task ActionAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption("Push 1 town", ctx => ctx.PushUpToNTokens(1,Invader.Town),ctx.Tokens.Has(Invader.Town)),
				new PowerOption("Push 3 dahan", ctx => ctx.PushUpToNTokens(3,TokenType.Dahan),ctx.HasDahan)
			);

		}
	}
}
