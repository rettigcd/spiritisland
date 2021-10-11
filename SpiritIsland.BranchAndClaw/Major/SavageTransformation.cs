using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SavageTransformation {

		[MajorCard( "Savage Transformation", 2, Element.Moon, Element.Animal )]
		[Slow]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// 2 fear
			ctx.AddFear(2);

			// replace 1 explorer with 1 beast
			if(ctx.Tokens.Has( Invader.Explorer ))
				ReplaceExplorerWithBeast( ctx );

			// if you have 2 moon, 3 animal: 
			if(ctx.YouHave("2 moon,3 animal" )) {
				// replace 1 additional explorer with 1 beat in either target or adjacent land
				var secondSpaceCtx = await ctx.SelectAdjacentLandOrSelf( "convert 2nd explorer to beast", x=>x.Tokens.Has(Invader.Explorer) );
				if(secondSpaceCtx != null )
					ReplaceExplorerWithBeast( secondSpaceCtx );
			}
		}

		private static void ReplaceExplorerWithBeast( TargetSpaceCtx ctx ) {
			ctx.Tokens[Invader.Explorer.Default]--;
			ctx.Tokens.Beasts().Count++;
		}
	}
}
