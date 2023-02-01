namespace SpiritIsland;

public class TokenPusher {

	public TokenPusher( TargetSpaceCtx ctx ) {
		this.ctx = ctx;
		this.source = ctx.Space;
	}

	public TokenPusher AddGroup(int count,params TokenClass[] groups ) {

		count = System.Math.Min( count, ctx.GameState.Tokens[source].SumAny(groups) );

		int index = sharedGroupCounts.Count;
		sharedGroupCounts.Add( count );
		foreach(var group in groups) 
			indexLookupByGroup.Add( group, index );

		return this; // chain together
	}

	public Task<Space[]> MoveN() => Exec( Present.Always );
	public Task<Space[]> MoveUpToN() => Exec( Present.Done );

	/// <returns>Spaces pushed too.</returns>
	async Task<Space[]> Exec( Present present ) {

		var counts = ctx.Target(source).Tokens;
		IVisibleToken[] GetTokens() {
			var groupsWithRemainingCounts = indexLookupByGroup
				.Where( pair => sharedGroupCounts[pair.Value] > 0 )
				.Select( p => p.Key )
				.ToArray();
			return counts.OfAnyClass( groupsWithRemainingCounts ).Cast<IVisibleToken>().ToArray(); // !!! Make Dahan Freezable
		}

		var pushedToSpaces = new List<Space>();

		IVisibleToken[] tokens;
		while(0 < (tokens = GetTokens()).Length) {
			// Select Token
			var token = (await ctx.Self.Gateway.Decision( Select.TokenFrom1Space.TokenToPush( source, sharedGroupCounts.Sum(), tokens, present ) ))?.Token;
			if(token == null) break;

			// Push to Destination
			Space destination = await PushToken( token );

			// Book keeping
			--sharedGroupCounts[indexLookupByGroup[token.Class]]; // decrement count
			if(destination != null)
				pushedToSpaces.Add( destination ); // record push
		}
		return pushedToSpaces.ToArray();
	}

	public async Task<Space> PushToken( IVisibleToken token ) {
		Space destination = await SelectDestination( token );
		if(destination == null) return null;

		await MoveSingleToken( token, source, destination );

		return destination;
	}

	protected virtual async Task MoveSingleToken( IToken token, Space source, Space destination ) {
		await ctx.Move( token, source, destination );    // !!! if moving into frozen land, freeze Dahan
		if( _customAction != null )
			await _customAction( token, source, destination );
	}


	public TokenPusher AddCustomMoveAction( Func<IToken,Space,Space,Task> customeAction ) { // !!! The args could be the move event, why aren't we using that event instead of this?
		this._customAction = customeAction;
		return this;
	}
	Func<IToken,Space,Space,Task> _customAction;

	protected virtual async Task<Space> SelectDestination( IVisibleToken token ) {
		IEnumerable<SpaceState> destinationOptions = ctx.GameState.Tokens[source].Adjacent
			.Where( ctx.TerrainMapper.IsInPlay );
		foreach(var filter in destinationFilters)
			destinationOptions = destinationOptions.Where(filter);

		return await ctx.Decision( Select.Space.PushToken( token, source, destinationOptions, Present.Always ) );
	}

	public TokenPusher FilterDestinations(Func<SpaceState,bool> destinationFilter ) {
		destinationFilters.Add(destinationFilter);
		return this;
	}

	#region private

	protected readonly SelfCtx ctx;
	protected readonly Space source;
	protected readonly List<Func<SpaceState,bool>> destinationFilters = new List<Func<SpaceState, bool>>();

	// if we push 3 explorer/town,
	// sharedGroupCounts[0] = 3
	// indexLookup[Explorer] = 0 (index)
	// indexLookup[Town] = 0 (index)

	readonly List<int> sharedGroupCounts = new(); // the # we push from each group

	readonly Dictionary<TokenClass, int> indexLookupByGroup = new(); // map from group back to its count

	#endregion

}