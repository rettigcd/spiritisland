using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class OvergrowInANight {

		[SpiritCard( "Overgrow in a Night", 2, Speed.Fast, Element.Moon, Element.Plant )]
		[FromPresence( 1 )]
		static public async Task ActionAsync( ActionEngine eng, Space target ) {

			const string addFearText = "3 fear";
			bool addFear = eng.Self.Presence.IsOn(target)
				&& eng.GameState.HasInvaders(target)
				&& await eng.SelectText( "Select power", "add 1 presence", addFearText ) == addFearText;

			if( addFear )
				eng.GameState.AddFear(3);
			else
				await eng.PlacePresence( target );
		}

	}
}
