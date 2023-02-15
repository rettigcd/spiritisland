namespace SpiritIsland;

public class TokenPusher {

	public TokenPusher( Spirit self, SpaceState tokens ) {
		_self = self;
		_tokens = tokens;
	}

	public TokenPusher AddGroup(int count,params IEntityClass[] groups ) {

		count = System.Math.Min( count, _tokens.SumAny(groups) );

		int index = sharedGroupCounts.Count;
		sharedGroupCounts.Add( count );

		foreach(IEntityClass group in groups) 
			indexLookupByGroup.Add( group, index );

		return this; // chain together
	}

	public Task<Space[]> MoveN() => Exec( Present.Always );
	public virtual Task<Space[]> MoveUpToN() => Exec( Present.Done );

	/// <returns>Spaces pushed too.</returns>
	async Task<Space[]> Exec( Present present ) {

		var counts = _tokens;
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
			var token = (await _self.Gateway.Decision( Select.TokenFrom1Space.TokenToPush( _tokens.Space, sharedGroupCounts.Sum(), tokens, present ) ))?.Token;
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

		await MoveSingleToken( token, _tokens, destination );

		return destination;
	}

	protected virtual async Task MoveSingleToken( IToken token, SpaceState source, SpaceState destination ) {
		TokenMovedArgs moveResult = await token.Move( source, destination );
		if( _customAction != null )
			await _customAction( moveResult );
	}


	public TokenPusher OnMove( Func<TokenMovedArgs, Task> customeAction ) { // !!! The args could be the move event, why aren't we using that event instead of this?
		_customAction = customeAction;
		return this;
	}
	Func<TokenMovedArgs, Task> _customAction;

	protected virtual async Task<Space> SelectDestination( IToken token ) {
		IEnumerable<SpaceState> destinationOptions = _tokens.Adjacent;
		foreach(var filter in destinationFilters)
			destinationOptions = destinationOptions.Where(filter);

		return await _self.Gateway.Decision( Select.ASpace.PushToken( token, _tokens.Space, destinationOptions, Present.Always ) );
	}

	public TokenPusher FilterDestinations(Func<SpaceState,bool> destinationFilter ) {
		destinationFilters.Add(destinationFilter);
		return this;
	}

	#region private

	protected readonly Spirit _self;
	protected readonly SpaceState _tokens;

	protected readonly List<Func<SpaceState,bool>> destinationFilters = new List<Func<SpaceState, bool>>();

	// if we push 3 explorer/town,
	// sharedGroupCounts[0] = 3
	// indexLookup[Explorer] = 0 (index)
	// indexLookup[Town] = 0 (index)

	readonly List<int> sharedGroupCounts = new(); // the # we push from each group

	readonly Dictionary<IEntityClass, int> indexLookupByGroup = new(); // map from group back to its count

	#endregion

}