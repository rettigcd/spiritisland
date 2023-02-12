namespace SpiritIsland;

public class TokenPusher {

	public TokenPusher( TargetSpaceCtx ctx ) {
		_ctx = ctx;
		_source = ctx.Space;
	}

	public TokenPusher AddGroup(int count,params IEntityClass[] groups ) {

		count = System.Math.Min( count, _ctx.GameState.Tokens[_source].SumAny(groups) );

		int index = sharedGroupCounts.Count;
		sharedGroupCounts.Add( count );

		foreach(IEntityClass group in groups) 
			indexLookupByGroup.Add( group, index );

		return this; // chain together
	}

	public Task<Space[]> MoveN() => Exec( Present.Always );
	public Task<Space[]> MoveUpToN() => Exec( Present.Done );

	/// <returns>Spaces pushed too.</returns>
	async Task<Space[]> Exec( Present present ) {

		var counts = _ctx.Target(_source).Tokens;
		IToken[] GetTokens() {
			var groupsWithRemainingCounts = indexLookupByGroup
				.Where( pair => 0 < sharedGroupCounts[pair.Value] )
				.Select( p => p.Key )
				.ToArray();
			return counts.RemovableOfAnyClass( RemoveReason.MovedFrom, groupsWithRemainingCounts )
				.Cast<IToken>()
				.ToArray();
		}

		var pushedToSpaces = new List<Space>();

		IToken[] tokens;
		while(0 < (tokens = GetTokens()).Length) {
			// Select Token
			var token = (await _ctx.Self.Gateway.Decision( Select.TokenFrom1Space.TokenToPush( _source, sharedGroupCounts.Sum(), tokens, present ) ))?.Token;
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

	public async Task<Space> PushToken( IToken token ) {
		Space destination = await SelectDestination( token );
		if(destination == null) return null;

		await MoveSingleToken( token, _source, destination );

		return destination;
	}

	protected virtual async Task MoveSingleToken( IToken token, Space source, Space destination ) {
		await _ctx.Move( token, source, destination );
		if( _customAction != null )
			await _customAction( token, source, destination );
	}


	public TokenPusher AddCustomMoveAction( Func<ISpaceEntity,Space,Space,Task> customeAction ) { // !!! The args could be the move event, why aren't we using that event instead of this?
		_customAction = customeAction;
		return this;
	}
	Func<ISpaceEntity,Space,Space,Task> _customAction;

	protected virtual async Task<Space> SelectDestination( IToken token ) {
		IEnumerable<SpaceState> destinationOptions = _ctx.GameState.Tokens[_source].Adjacent;
		foreach(var filter in destinationFilters)
			destinationOptions = destinationOptions.Where(filter);

		return await _ctx.Decision( Select.ASpace.PushToken( token, _source, destinationOptions, Present.Always ) );
	}

	public TokenPusher FilterDestinations(Func<SpaceState,bool> destinationFilter ) {
		destinationFilters.Add(destinationFilter);
		return this;
	}

	#region private

	protected readonly SelfCtx _ctx;
	protected readonly Space _source;
	protected readonly List<Func<SpaceState,bool>> destinationFilters = new List<Func<SpaceState, bool>>();

	// if we push 3 explorer/town,
	// sharedGroupCounts[0] = 3
	// indexLookup[Explorer] = 0 (index)
	// indexLookup[Town] = 0 (index)

	readonly List<int> sharedGroupCounts = new(); // the # we push from each group

	readonly Dictionary<IEntityClass, int> indexLookupByGroup = new(); // map from group back to its count

	#endregion

}