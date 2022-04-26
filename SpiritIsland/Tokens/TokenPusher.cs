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
		Token[] GetTokens() {
			var groupsWithRemainingCounts = indexLookupByGroup
				.Where( pair => sharedGroupCounts[pair.Value] > 0 )
				.Select( p => p.Key )
				.ToArray();
			return counts.OfAnyType( groupsWithRemainingCounts ); // !!! Make Dahan Freezable
		}

		var pushedToSpaces = new List<Space>();

		Token[] tokens;
		while(0 < (tokens = GetTokens()).Length) {
			// Select Token
			var decision = Select.TokenFrom1Space.TokenToPush( source, sharedGroupCounts.Sum(), tokens, present );
			var token = await ctx.Self.Action.Decision( decision );
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

	public async Task<Space> PushToken( Token token ) {
		Space destination = await SelectDestination( token );
		if(destination == null) return null;

		await ctx.Move( token, source, destination );	// !!! if moving into frozen land, freeze Dahan

		customAction?.Invoke( token, source, destination);

		return destination;
	}

	public TokenPusher AddCustomMoveAction( Func<Token,Space,Space,Task> customeAction ) {
		this.customAction = customeAction;
		return this;
	}
	Func<Token,Space,Space,Task> customAction;

	protected virtual async Task<Space> SelectDestination( Token token ) {
		IEnumerable<Space> destinationOptions = source.Adjacent.Where( s => ctx.Target(s).IsInPlay(token.Class) );
		foreach(var filter in destinationFilters)
			destinationOptions = destinationOptions.Where(filter);

		return await ctx.Decision( Select.Space.PushToken( token, source, destinationOptions, Present.Always ) );
	}

	public TokenPusher FilterDestinations(Func<Space,bool> destinationFilter ) {
		destinationFilters.Add(destinationFilter);
		return this;
	}

	#region private

	protected readonly SelfCtx ctx;
	protected readonly Space source;
	protected readonly List<Func<Space,bool>> destinationFilters = new List<Func<Space, bool>>();

	// if we push 3 explorer/town,
	// sharedGroupCounts[0] = 3
	// indexLookup[Explorer] = 0 (index)
	// indexLookup[Town] = 0 (index)

	readonly List<int> sharedGroupCounts = new(); // the # we push from each group

	readonly Dictionary<TokenClass, int> indexLookupByGroup = new(); // map from group back to its count

	#endregion

}