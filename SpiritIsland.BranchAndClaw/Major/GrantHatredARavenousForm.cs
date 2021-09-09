using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GrantHatredARavenousForm {

		[MajorCard( "Grant Hatred a Ravenous Form", 4, Speed.Slow, Element.Moon, Element.Fire )]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			bool hasInvaders = ctx.HasInvaders;

			// for each strife or blight in target land, 1 fear and 2 damage.
			int count = ctx.Tokens.Keys.OfType<StrifedInvader>().Sum(x=>x.StrifeCount * ctx.Tokens[x])
				+ ctx.Tokens.Blight.Count;
			ctx.AddFear( count );
			await ctx.DamageInvaders( count * 2 );

			// if this destorys all invaders in target land, add 1 beast.
			if(hasInvaders && !ctx.HasInvaders)
				ctx.Tokens.Beasts().Count++;

			// if you have 4 moon, 2 fire
			if(ctx.Self.Elements.Contains("4 moon,2 fire" )) {
				// add 1 strife in up to 3 adjacent lands.
				TokenCountDictionary[] tokenSpaces = ctx.PowerAdjacents()
					.Select(x=>ctx.GameState.Tokens[x])
					.Where(tokens=> tokens.HasInvaders())
					.Take(3)
					.ToArray();

				foreach(var tokens in tokenSpaces)
					await ctx.Self.SelectInvader_ToStrife( tokens );
			}
		}

	}

}
