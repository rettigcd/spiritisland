using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class Fleshrot_Fever {

		[MinorCard( "Fleshrot Fever", 1, Speed.Slow, Element.Fire, Element.Air, Element.Water, Element.Animal )]
		[FromPresence( 1, Target.JungleOrSand )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			ctx.Tokens[BacTokens.Disease]++;
			return Task.CompletedTask;
		}

	}

}
