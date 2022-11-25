namespace SpiritIsland;

public class ReadOnlyBoundedPresence {

}

/// <summary> High level Presence Methods for API </summary>
public class BoundPresence : IKnowSpiritLocations {

	#region constructor

	public BoundPresence(SelfCtx ctx) { 
		_self = ctx.Self;
		_gameState = ctx.GameState;
		_terrainMapper = ctx.TerrainMapper;
		_actionId = ctx.CurrentActionId;
	}

	readonly Spirit _self;
	readonly GameState _gameState;
	readonly TerrainMapper _terrainMapper;
	readonly Guid _actionId;
	SpiritPresence _inner       => _self.Presence;

	#endregion

	public void Move( Space from, Space to ) => _inner.Move(from,to, _gameState ); // !!! should have an ActionID
	public void PlaceOn( Space space ) => _inner.PlaceOn( _gameState.Tokens[space] ); // !!! this should take an action ID
	public Task Destroy( Space space, DestoryPresenceCause actionType ) => _inner.Destroy( space, _gameState, actionType, _actionId );
	public Task RemoveFrom( Space space ) => _inner.RemoveFrom( space, _gameState ); // Generally used for Replacing, !!! should have an Action ID

	public bool CanBePlacedOn( Space space ) => _inner.CanBePlacedOn( _gameState.Tokens[space], _terrainMapper );
	public bool IsSacredSite( Space space ) => _inner.IsSacredSite( _gameState.Tokens[space] );


	public IEnumerable<Space> Spaces => _inner.Spaces( _gameState ); // !!! Move everything that calls this over to SpaceStates, then remove this.
	public IEnumerable<SpaceState> SpaceStates => _inner.SpaceStates(_gameState);
	public IEnumerable<SpaceState> SacredSites => _inner.SacredSiteStates( _gameState, _terrainMapper );

	#region Higher Order

	// !!! should have an action ID
	public async Task<(Space, Space)> PushUpTo1() {
		// Select source
		var source = await _self.Action.Decision( Select.DeployedPresence.ToPush( _self, _gameState ) );
		if(source == null) return (null, null);

		// Select destination
		var destination = await _self.Action.Decision( Select.Space.PushPresence( source, _gameState.Tokens[source].Adjacent, Present.Always ) );
		Move( source, destination );
		return (source, destination);
	}

	/// <summary> Selects: (Source then Destination) for placing presence </summary>
	/// <remarks> Called from normal PlacePresence Growth + Gift of Proliferation. </remarks>
	/// !!! should Have an Action ID
	public async Task<(IOption,Space)> PlaceWithin( TargetCriteria targetCriteria, TargetingPowerType targetingPowerType ) {
		IOption from = await SelectSource();
		Space to = await SelectDestinationWithinRange( targetCriteria, targetingPowerType );
		await _self.Presence.Place( from, to, _gameState );
		return(from, to);
	}

	/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
	/// <returns>Place in Ocean, Growth through sacrifice</returns>
	/// !!! should Have an Action ID
	public async Task Place( params Space[] destinationOptions ) {
		var from = await SelectSource();
		var to = await _self.Action.Decision( Select.Space.ToPlacePresence( destinationOptions, Present.Always ) );
		await _self.Presence.Place( from, to, _gameState );
	}

	/// !!! should Have an Action ID
	public async Task DestroyOneFromAnywhere( DestoryPresenceCause actionType, Func<SpiritIsland.SpaceState, bool> filter = null ) {
		var space = filter == null
			? await _self.Action.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", _self, _gameState ) )
			: await _self.Action.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", _self, _gameState, filter ) );
		await Destroy( space, actionType );
	}


	#endregion

	#region Restore Destroyed

	public async Task ReturnUpToNDestroyedToTrack( int count ) {
		count = Math.Max(count,_self.Presence.Destroyed);
		while(count > 0) {
			var dst = await _self.Action.Decision( Select.TrackSlot.ToCover( _self ) );
			if(dst == null) break;
			await _self.Presence.ReturnDestroyedToTrack(dst);
			--count;
		}
	}

	#endregion

	#region select Source

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	public async Task<IOption> SelectSource(string actionPhrase = "place") {
		string prompt = $"Select Presence to {actionPhrase}.";
		return (IOption)await _self.Action.Decision( Select.TrackSlot.ToReveal( prompt, _self, _gameState ) )
			?? (IOption)await _self.Action.Decision( Select.DeployedPresence.All( prompt, _self, _gameState, Present.Always) );
	}

	public Task<Space> SelectDeployed(string prompt)
		=> _self.Action.Decision( Select.DeployedPresence.All(prompt, _self, _gameState, Present.Always ) );

	public Task<Space> SelectSacredSite(string prompt)
		=> _self.Action.Decision( Select.DeployedPresence.SacredSites(prompt, _gameState, _self, _terrainMapper, Present.Always ) );

	#endregion

	#region select Destination

	/// <summary>Selects a Space within a range of spirits Presence</summary>
	/// <param name="targetingPowerType">
	/// None => standard ranging 
	/// Innate/PowerCard => power ranging  + Passes to PowerRanging
	/// </param>
	public async Task<Space> SelectDestinationWithinRange( TargetCriteria targetCriteria, TargetingPowerType targetingPowerType ) {
		var options = FindSpacesWithinRange( targetCriteria, targetingPowerType ).ToArray();
		return await _self.Action.Decision( Select.Space.ToPlacePresence( options, Present.Always ) );
	}

	public IEnumerable<Space> FindSpacesWithinRange( TargetCriteria targetCriteria, TargetingPowerType targetingPowerType ) {
		var rangeCalculator = targetingPowerType switch {
			TargetingPowerType.None => DefaultRangeCalculator.Singleton,
			TargetingPowerType.Innate => _self.PowerRangeCalc,
			TargetingPowerType.PowerCard => _self.PowerRangeCalc,
			_ => throw new InvalidOperationException()
		};

		var options = rangeCalculator
			.GetTargetOptionsFromKnownSource( _self, _terrainMapper, targetingPowerType, SpaceStates, targetCriteria )
			.Where( CanBePlacedOn );
		return options;
	}

	#endregion

}
