using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class Fleshrot_Fever {

		[MinorCard( "Fleshrot Fever", 1, Element.Fire, Element.Air, Element.Water, Element.Animal )]
		[Slow]
		[FromPresence( 1, Target.JungleOrSand )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			ctx.Tokens.Disease.Count++;
			return Task.CompletedTask;
		}

	}

}
