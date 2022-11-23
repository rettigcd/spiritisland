namespace SpiritIsland;

/// <summary> High level Presence Methods for API </summary>
public class BoundPresence : IKnowSpiritLocations {

	#region constructor

	public BoundPresence(SelfCtx ctx) { this.ctx = ctx; }
	readonly SelfCtx ctx;
	SpiritPresence _inner => ctx.Self.Presence;
	GameState _gameState => ctx.GameState;

	#endregion

	public void Move( Space from, Space to ) => _inner.Move(from,to, _gameState );
	public void PlaceOn( Space space ) => _inner.PlaceOn( _gameState.Tokens[space] );
	public Task Destroy( Space space, DestoryPresenceCause actionType ) => _inner.Destroy( space, _gameState, actionType );
	public Task RemoveFrom( Space space ) => _inner.RemoveFrom( space, _gameState ); // Generally used for Replacing
	public bool CanBePlacedOn( Space space ) => _inner.CanBePlacedOn( ctx.TerrainMapper, _gameState.Tokens[space] );
	public bool IsSacredSite( Space space ) => _inner.IsSacredSite( _gameState.Tokens[space] );


	public IEnumerable<Space> Spaces => _inner.Spaces( _gameState ); // !!! Move everything that calls this over to SpaceStates, then remove this.
	public IEnumerable<SpaceState> SpaceStates => _inner.SpaceStates(_gameState);
	public IEnumerable<SpaceState> SacredSites => _inner.SacredSiteStates( _gameState, ctx.TerrainMapper );

	#region Higher Order

	public async Task<(Space, Space)> PushUpTo1() {
		// Select source
		var source = await ctx.Decision( Select.DeployedPresence.ToPush( ctx.Self, ctx.GameState ) );
		if(source == null) return (null, null);
		var sourceCtx = ctx.Target( source );
		// Select destination
		var destination = await sourceCtx.Decision( Select.Space.PushPresence( sourceCtx.Space, sourceCtx.Tokens.Adjacent, Present.Always ) );
		Move( source, destination );
		return (source, destination);
	}

	/// <summary> Selects: (Source then Destination) for placing presence </summary>
	/// <remarks> Called from normal PlacePresence Growth + Gift of Proliferation. </remarks>
	public async Task<(IOption,Space)> PlaceWithin( int range, string filterEnum ) {
		IOption from = await SelectSource();
		Space to = await SelectDestinationWithinRange( range, filterEnum );
		await ctx.Self.Presence.Place( from, to, ctx.GameState );
		return(from, to);
	}

	/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
	/// <returns>Place in Ocean, Growth through sacrifice</returns>
	public async Task Place( params Space[] destinationOptions ) {
		var from = await SelectSource();
		var to = await ctx.Decision( Select.Space.ToPlacePresence( destinationOptions, Present.Always ) );
		await ctx.Self.Presence.Place( from, to, ctx.GameState );
	}

	public async Task DestroyOneFromAnywhere( DestoryPresenceCause actionType, Func<SpiritIsland.SpaceState, bool> filter = null ) {
		var space = filter == null
			? await ctx.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", ctx.Self, ctx.GameState ) )
			: await ctx.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", ctx.Self, ctx.GameState, filter ) );
		await Destroy( space, actionType );
	}


	#endregion

	#region Restore Destroyed

	public async Task ReturnUpToNDestroyedToTrack( int count ) {
		count = Math.Max(count,ctx.Self.Presence.Destroyed);
		while(count > 0) {
			var dst = await ctx.Decision( Select.TrackSlot.ToCover( ctx.Self ) );
			if(dst == null) break;
			await ctx.Self.Presence.ReturnDestroyedToTrack(dst,ctx.GameState);
			--count;
		}
	}

	#endregion

	#region select Source

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	public async Task<IOption> SelectSource(string actionPhrase = "place") {
		string prompt = $"Select Presence to {actionPhrase}.";
		return (IOption)await ctx.Decision( Select.TrackSlot.ToReveal( prompt, ctx.Self, ctx.GameState ) )
			?? (IOption)await ctx.Decision( Select.DeployedPresence.All( prompt, ctx.Self, ctx.GameState, Present.Always) );
	}

	public Task<Space> SelectDeployed(string prompt)
		=> ctx.Decision( Select.DeployedPresence.All(prompt, ctx.Self, ctx.GameState, Present.Always ) );

	public Task<Space> SelectSacredSite(string prompt)
		=> ctx.Decision( Select.DeployedPresence.SacredSites(prompt, ctx.GameState, ctx.Self, ctx.TerrainMapper, Present.Always ) );

	#endregion

	#region select Destination

	/// <summary> Selects a space within [range] of current presence </summary>
	public async Task<Space> SelectDestinationWithinRange( int range, string filterEnum ) {
		var options = GetValidDestinationOptionsFromPresence(range,filterEnum, SpaceStates );
		return await ctx.Decision( Select.Space.ToPlacePresence( options, Present.Always ) );
	}

	/// <summary> Select a space withing [range] of specified spaces </summary>
	IEnumerable<Space> GetValidDestinationOptionsFromPresence( int range, string filterEnum, IEnumerable<SpaceState> source ) {
		return ctx.Self.RangeCalc.GetTargetOptionsFromKnownSource( ctx, TargettingFrom.None, source, new TargetCriteria( range, filterEnum) )
			.Where( CanBePlacedOn );
	}

	#endregion

}
