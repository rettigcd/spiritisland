using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VoiceOfThunder {

		[SpiritCard( "Voice of Thunder", 0, Element.Sun, Element.Air )]
		[Slow]
		[FromPresence(1)]
		static public Task Act( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption("push up to 4 dahan", () => ctx.PushUpToNDahan( 4 ), ctx.HasDahan ),
				new ActionOption("2 fear", () => ctx.AddFear(2), ctx.Tokens.HasInvaders() ) 
			);

		}
	}
}
