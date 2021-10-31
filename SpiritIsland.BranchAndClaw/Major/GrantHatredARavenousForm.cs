using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GrantHatredARavenousForm {

		[MajorCard( "Grant Hatred a Ravenous Form", 4, Element.Moon, Element.Fire )]
		[Slow]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			bool originallyHadInvaders = ctx.HasInvaders;

			// for each strife or blight in target land, 
			int count = ctx.Tokens.Keys.OfType<StrifedInvader>().Sum(x=>x.StrifeCount * ctx.Tokens[x])
				+ ctx.Blight.Count;
			// 1 fear 
			ctx.AddFear( count );
			// and 2 damage.
			await ctx.DamageInvaders( count * 2 );

			// if this destorys all invaders in target land, add 1 beast.
			if(originallyHadInvaders && !ctx.HasInvaders)
				ctx.Beasts.Count++;

			// if you have 4 moon, 2 fire
			if(await ctx.YouHave("4 moon,2 fire")) {
				// add 1 strife in up to 3 adjacent lands.
				var tokenSpaces = ctx.Adjacent
					.Where(s => ctx.Target(s).HasInvaders)
					.ToList();
				for(int i = 0; tokenSpaces.Count >0 && i < 3; ++i) {
					var space = await ctx.Self.Action.Decision(new Decision.TargetSpace("Add Strife", tokenSpaces, Present.Done));
					await ctx.AddStrife();
					tokenSpaces.Remove(space);
				}

			}
		}

	}

}
