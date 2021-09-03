using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EnticingSplendor {

		[MinorCard( "Enticing Splendor", 0, Speed.Fast, Element.Sun, Element.Air, Element.Plant )]
		[FromPresence( 0, Target.NoBlight )]
		public static Task ActAsync( TargetSpaceCtx ctx) {

			return ctx.SelectPowerOption(
				new PowerOption( "Gather 1 explorer/town", () => ctx.GatherUpToNTokens( 1, Invader.Explorer, Invader.Town ) ),
				new PowerOption( "Gather 2 dahan", () => ctx.GatherUpToNDahan( 2 ) )
			);

		}
	}


}
