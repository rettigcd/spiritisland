using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InflameTheFiresOfLife {

		[MinorCard( "Inflame the Fires of Life", 1, Speed.Slow, Element.Moon, Element.Fire, Element.Plant, Element.Animal )]
		[FromSacredSite( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new ActionOption( "add disease ", () => ctx.Tokens.Disease().Count++ ),
				new ActionOption( "1 fear and 1 strife", () => FearAndStrife(ctx) )
			);
		}

		static Task FearAndStrife( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			return ctx.AddStrife();
		}


	}

}
