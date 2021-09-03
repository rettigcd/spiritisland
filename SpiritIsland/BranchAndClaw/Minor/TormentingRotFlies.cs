using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class TormentingRotFlies {

		[MinorCard( "Tormenting Rotflies", 1, Speed.Slow, Element.Air, Element.Plant, Element.Animal )]
		[FromPresence( 2, Target.SandOrWetland )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectPowerOption(
				new PowerOption( "Add 1 disease", ctx => ctx.Tokens[BacTokens.Disease]++ ),
				new PowerOption( "2 fear, +1(if disease) +1(if blight)", AddFear, ctx.Tokens.HasInvaders() )
			);

		}

		static public void AddFear( TargetSpaceCtx ctx ) {
			int fearCount = 2;
			if( ctx.Tokens.Has( BacTokens.Disease)) fearCount++;
			if( ctx.Tokens.Has( TokenType.Blight )) fearCount++;
			ctx.AddFear( fearCount );
		}
	}
}
