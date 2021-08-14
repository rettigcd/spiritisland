using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class EnticingSplendor {
		[MinorCard( "Enticing Splendor", 0, Speed.Fast, Element.Sun, Element.Air, Element.Plant )]
		[FromPresence( 0, Target.NoBlight )]
		public static async Task ActAsync( ActionEngine eng, Space target ) {
			const string dahanText = "2 dahan";
			if(dahanText == await eng.SelectText( "Gather what?", dahanText, "1 Explorer/Town" ))
				await eng.GatherUpToNDahan( target, 2 );
			else
				await eng.GatherUpToNInvaders( target, 1, Invader.Explorer, Invader.Town );
		}
	}


}
