using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VoiceOfThunder {

		[SpiritCard( "Voice of Thunder", 0, Speed.Slow, Element.Sun, Element.Air )]
		[FromPresence(1)]
		static public async Task Act( ActionEngine engine, Space target ) {
			// push up to 4 dahan -OR- If invaders are present, 2 fear

			const string fearOption = "2 fear";
			bool doFear = engine.GameState.HasInvaders( target ) 
				&& await engine.SelectText( "Chose card option", fearOption, "push up to 4 dahan" ) == fearOption;

			if( doFear )
				engine.AddFear( 2 );
			else
				await engine.PushUpToNDahan(target,4);
		}
	}
}
