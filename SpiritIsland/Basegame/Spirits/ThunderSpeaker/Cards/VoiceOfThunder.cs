using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VoiceOfThunder {

		[SpiritCard( "Voice of Thunder", 0, Speed.Slow, Element.Sun, Element.Air )]
		[FromPresence(1)]
		static public async Task Act( TargetSpaceCtx ctx ) {
			// push up to 4 dahan -OR- If invaders are present, 2 fear

			const string fearOption = "2 fear";
			bool doFear = ctx.Tokens.HasInvaders()
				&& await ctx.Self.SelectText( "Chose card option", fearOption, "push up to 4 dahan" ) == fearOption;

			if( doFear )
				ctx.AddFear( 2 );
			else
				await ctx.PowerPushUpToNTokens(4, TokenType.Dahan );
		}
	}
}
