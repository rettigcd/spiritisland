using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EnticingSplendor {

		[MinorCard( "Enticing Splendor", 0, Element.Sun, Element.Air, Element.Plant )]
		[Fast]
		[FromPresence( 0, Target.NoBlight )]
		public static Task ActAsync( TargetSpaceCtx ctx) {

			return ctx.SelectActionOption(
				new ActionOption( "Gather 1 explorer/town", () => ctx.GatherUpTo( 1, Invader.Explorer, Invader.Town ) ),
				new ActionOption( "Gather up to 2 dahan", () => ctx.GatherUpToNDahan( 2 ) )
			);

		}
	}


}
