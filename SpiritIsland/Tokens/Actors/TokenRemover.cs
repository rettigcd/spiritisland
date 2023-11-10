namespace SpiritIsland;

public class TokenRemover {

	public TokenRemover( TargetSpaceCtx ctx ) {
		_ctx = ctx;
	}

	public TokenRemover AddGroup(int count,params IEntityClass[] groups ) {

		count = System.Math.Min( count, _ctx.Tokens.SumAny(groups) );

		int index = sharedGroupCounts.Count;
		sharedGroupCounts.Add( count );
		foreach(var group in groups) 
			indexLookupByGroup.Add( group, index );

		return this; // chain together
	}

	public Task RemoveN() => Exec( Present.Always );
	public Task RemoveUpToN() => Exec( Present.Done );

	async Task Exec( Present present ) {

		var counts = _ctx.Tokens;
		IToken[] GetTokens() {
			var groupsWithRemainingCounts = indexLookupByGroup
				.Where( pair => sharedGroupCounts[pair.Value] > 0 )
				.Select( p => p.Key )
				.ToArray();
			return counts.OfAnyClass( groupsWithRemainingCounts )
				.Cast<IToken>()
				.ToArray();
		}

		IToken[] tokens;
		while(0 < (tokens = GetTokens()).Length) {
			// Select Token
			var token = (await _ctx.Self.Select( A.SpaceToken.ToRemove( sharedGroupCounts.Sum(), tokens.OnOne( _ctx.Space ), present ) ))?.Token;
			if(token == null) break;

			// Remove
			await _ctx.Remove( token, 1, RemoveReason.Removed);

			// Book keeping
			--sharedGroupCounts[indexLookupByGroup[token.Class]]; // decrement count
		}

	}

	#region private

	protected readonly TargetSpaceCtx _ctx;
	protected readonly List<Func<Space,bool>> destinationFilters = new List<Func<Space, bool>>();

	// if we remove 3 explorer/town,
	// sharedGroupCounts[0] = 3
	// indexLookup[Explorer] = 0 (index)
	// indexLookup[Town] = 0 (index)

	readonly List<int> sharedGroupCounts = new(); // the # we push from each group

	readonly Dictionary<IEntityClass, int> indexLookupByGroup = new(); // map from group back to its count

	#endregion

}