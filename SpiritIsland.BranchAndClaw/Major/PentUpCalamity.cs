using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PentUpCalamity {

		[MajorCard( "Pent-Up Calamity", 3, Element.Moon, Element.Fire, Element.Earth, Element.Plant, Element.Animal )]
		[Fast]
		[FromPresence( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.SelectActionOption(
				new ActionOption("Add 1 disease and 1 strife", () => AddDiseaseAndStrife( ctx ) ),
				new ActionOption("Remove any # of beast/disease/strife/wilds for 1 fear + 3 damage each", () => RemoveTokensForFearAndDamage( ctx ) )
			);

		}

		static async Task AddDiseaseAndStrife( TargetSpaceCtx ctx ) {
			ctx.Disease.Add(1);
			await ctx.AddStrife();
		}

		static async Task RemoveTokensForFearAndDamage( TargetSpaceCtx ctx ) {
			var removed = new List<Token>();
			Token[] options = GetRemovableTokens( ctx );

			// if you have 2 moon, 3 fire: if you have remvoed tokens, return up to 2 of them.  Otherwise, add 2 strife
			// Instead of having Bonus return 2 tokens (like strife...), we will just not remove them
			bool hasBonus = await ctx.YouHave( "2 moon,3 fire" );
			int returnCount = hasBonus ? System.Math.Min(2,options.Length) : 0;

			while(options.Length > 0) {
				var tokenToRemove = await ctx.Self.Action.Decision( new Decision.TokenOnSpace(
					$"Remove token for (1 fear,3 damage) total({removed.Count},{removed.Count * 3})"
					, ctx.Space, options , Present.Done
				) );
				if(tokenToRemove == null) break;

				// If bonus allowed us to return some
				if(returnCount > 0)
					returnCount--;
				else
					RemoveToken( ctx, tokenToRemove );

				// Do fear now
				ctx.AddFear( 1 );
				// do damage later
				removed.Add(tokenToRemove);

				// Next
				options = GetRemovableTokens( ctx );
			}

			// now do damage all at once
			await ctx.DamageInvaders( removed.Count * 3 );

			// if you have 2 moon, 3 fire  but didn't remove any 
			if( hasBonus && removed.Count == 0 )
				// add 2 strife
				for(int i=0;i<2;++i)
					await ctx.AddStrife();
		}

		static void RemoveToken( TargetSpaceCtx ctx, Token tokenToRemove ) {
			if(tokenToRemove is StrifedInvader invader)
				ctx.Tokens.AddStrifeTo( invader, -1 );
			else
				ctx.Tokens[tokenToRemove]--;
		}

		static Token[] GetRemovableTokens( TargetSpaceCtx ctx ) {
			var options = ctx.Tokens.OfAnyType( TokenType.Beast.Generic, TokenType.Disease.Generic, TokenType.Wilds.Generic ).ToList();
			options.AddRange( ctx.Tokens.Keys.OfType<StrifedInvader>() );
			return options.ToArray();
		}

	}

}
