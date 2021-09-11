﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SavageTransformation {

		[MajorCard( "Savage Transformation", 2, Speed.Slow, Element.Moon, Element.Animal )]
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
				var neighborsWithExplorers = ctx.Adjacents.Where(s=>ctx.TargetSpace(s).Tokens.Has(Invader.Explorer)).ToList();
				if(ctx.Tokens.Has(Invader.Explorer)) neighborsWithExplorers.Add(ctx.Space);
				var secondSpace = await ctx.Self.Action.Decide(new TypedDecision<Space>("convert 2nd explorer to beast",neighborsWithExplorers,Present.Always)); // !!! simplify UI
				if( secondSpace != null )
					ReplaceExplorerWithBeast( ctx.TargetSpace( secondSpace ) );
			}
		}

		private static void ReplaceExplorerWithBeast( TargetSpaceCtx ctx ) {
			ctx.Tokens[Invader.Explorer.Default]--;
			ctx.Tokens.Beasts().Count++;
		}
	}
}
