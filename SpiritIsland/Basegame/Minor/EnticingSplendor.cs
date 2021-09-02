using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EnticingSplendor {

		[MinorCard( "Enticing Splendor", 0, Speed.Fast, Element.Sun, Element.Air, Element.Plant )]
		[FromPresence( 0, Target.NoBlight )]
		public static async Task ActAsync( TargetSpaceCtx ctx) {

			const string dahanText = "2 dahan";
			if(dahanText == await ctx.Self.SelectText( "Gather what?", dahanText, "1 Explorer/Town" ))
				await ctx.GatherUpToNDahan( 2 );
			else
				await ctx.GatherUpToNTokens( 1, Invader.Explorer, Invader.Town );
		}
	}


}
