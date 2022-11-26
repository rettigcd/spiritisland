namespace SpiritIsland;

public class TokenRemover {

	public TokenRemover( TargetSpaceCtx ctx ) {
		this.ctx = ctx;
		this.source = ctx.Space;
	}

	public TokenRemover AddGroup(int count,params TokenClass[] groups ) {

		count = System.Math.Min( count, ctx.GameState.Tokens[source].SumAny(groups) );

		int index = sharedGroupCounts.Count;
		sharedGroupCounts.Add( count );
		foreach(var group in groups) 
			indexLookupByGroup.Add( group, index );

		return this; // chain together
	}

	public Task RemoveN() => Exec( Present.Always );
	public Task RemoveUpToN() => Exec( Present.Done );

	async Task Exec( Present present ) {

		var counts = ctx.Target(source).Tokens;
		Token[] GetTokens() {
			var groupsWithRemainingCounts = indexLookupByGroup
				.Where( pair => sharedGroupCounts[pair.Value] > 0 )
				.Select( p => p.Key )
				.ToArray();
			return counts.OfAnyType( groupsWithRemainingCounts ); // !!! Make Dahan Freezable
		}

		Token[] tokens;
		while(0 < (tokens = GetTokens()).Length) {
			// Select Token
			var decision = Select.TokenFrom1Space.TokenToRemove( source, sharedGroupCounts.Sum(), tokens, present );
			var token = await ctx.Self.Gateway.Decision( decision );
			if(token == null) break;

			// Remove
			await ctx.Target(source).Remove( token, 1, RemoveReason.Removed);

			// Book keeping
			--sharedGroupCounts[indexLookupByGroup[token.Class]]; // decrement count
		}

	}

	#region private

	// !!! why doesn't this use a TargeSpaceCtx?

	protected readonly SelfCtx ctx;
	protected readonly Space source;
	protected readonly List<Func<Space,bool>> destinationFilters = new List<Func<Space, bool>>();

	// if we remove 3 explorer/town,
	// sharedGroupCounts[0] = 3
	// indexLookup[Explorer] = 0 (index)
	// indexLookup[Town] = 0 (index)

	readonly List<int> sharedGroupCounts = new(); // the # we push from each group

	readonly Dictionary<TokenClass, int> indexLookupByGroup = new(); // map from group back to its count

	#endregion

}