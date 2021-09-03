using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class Fleshrot_Fever {

		[MinorCard( "Fleshrot Fever", 1, Speed.Slow, Element.Fire, Element.Air, Element.Water, Element.Animal )]
		[FromPresence( 1, Target.JungleOrSand )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			var d = new InvadersOnSpaceDecision( "ff", ctx.Target, ctx.Tokens.Invaders().ToArray(), Present.Always );
			ctx.Tokens[BacTokens.Disease]++;
		}

	}

}
