namespace SpiritIsland.BranchAndClaw;

public class PentUpCalamity {

	[MajorCard( "Pent-Up Calamity", 3, Element.Moon, Element.Fire, Element.Earth, Element.Plant, Element.Animal ),Fast,FromPresence( 2 )]
	[Instructions( "Add 1 Disease and 1 Strife. -or- Remove any number of Beasts / Disease / Strife / Wilds. For each token removed, 1 Fear and 3 Damage. -If you have- 2 Moon, 3 Fire: If you have removed tokens, return up to 2 of them. Otherwise, add 2 Strife." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		await ctx.SelectActionOption(
			new SpaceCmd("Add 1 disease and 1 strife", AddDiseaseAndStrife ),
			new SpaceCmd("Remove any # of beast/disease/strife/wilds for 1 fear + 3 damage each", RemoveTokensForFearAndDamage )
		);

	}

	static async Task AddDiseaseAndStrife( TargetSpaceCtx ctx ) {
		await ctx.Disease.Add(1);
		await ctx.AddStrife();

		if( await ctx.YouHave( "2 moon,3 fire" ) ) {
			await ctx.AddStrife();
			await ctx.AddStrife();
		}

	}

	static async Task RemoveTokensForFearAndDamage( TargetSpaceCtx ctx ) {
		var removed = new List<ISpaceEntity>();
		IToken[] options = GetRemovableTokens( ctx );

		// if you have 2 moon, 3 fire: if you have remvoed tokens, return up to 2 of them.  Otherwise, add 2 strife
		// Instead of having Bonus return 2 tokens (like strife...), we will just not remove them
		bool hasBonus = await ctx.YouHave( "2 moon,3 fire" );
		int returnCount = hasBonus ? System.Math.Min(2,options.Length) : 0;

		while(options.Length > 0) {
			// This is kind of special purpose
			var removeTokenDecision = new Select.ASpaceToken(
				$"Remove token for (1 fear,3 damage) total({removed.Count},{removed.Count * 3})"
				, ctx.Space, options , Present.Done
			);
			var tokenToRemove = (await ctx.Decision( removeTokenDecision ))?.Token;
			if(tokenToRemove == null) break;

			// If bonus allowed us to return some
			if(returnCount > 0)
				returnCount--;
			else
				await RemoveToken( ctx, tokenToRemove );

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

	static async Task RemoveToken( TargetSpaceCtx ctx, IToken tokenToRemove ) {
		if(tokenToRemove is HumanToken invader)
			await ctx.Tokens.Remove1StrifeFrom( invader, 1 );
		else
			await ctx.Remove( tokenToRemove, 1 );
	}

	static IToken[] GetRemovableTokens( TargetSpaceCtx ctx ) {
		var options = ctx.Tokens.OfAnyClass( Token.Beast, Token.Disease, Token.Wilds ).ToList();
		options.AddRange( ctx.Tokens.OfTypeHuman() );
		return options.Cast<IToken>().ToArray();
	}

}