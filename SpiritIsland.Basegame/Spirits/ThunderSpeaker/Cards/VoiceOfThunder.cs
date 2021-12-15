using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VoiceOfThunder {

		[SpiritCard( "Voice of Thunder", 0, Element.Sun, Element.Air )]
		[Slow]
		[FromPresence(1)]
		static public Task Act( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new SpaceAction("push up to 4 dahan", ctx => ctx.PushUpToNDahan( 4 ), ctx.Dahan.Any ),
				new SpaceAction("2 fear", ctx => ctx.AddFear(2), ctx.Tokens.HasInvaders() ) 
			);

		}
	}
}
