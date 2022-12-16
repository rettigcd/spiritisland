namespace SpiritIsland;

public class ReadOnlyBoundPresence : IKnowSpiritLocations {

	#region Constructors
	public ReadOnlyBoundPresence( SelfCtx ctx ) {
		_self = ctx.Self;
		_gameState = ctx.GameState;
		_terrainMapper = ctx.TerrainMapper;
	}
	public ReadOnlyBoundPresence( Spirit self, GameState gameState, TerrainMapper terrainMapper ) {
		_self = self;
		_gameState = gameState;
		_terrainMapper = terrainMapper;
	}

	/// <summary> Constructs a ReadOnlyBoundPresence for POWER </summary>
	public ReadOnlyBoundPresence( Spirit self, GameState gameState ) {
		_self = self;
		_gameState = gameState;
		_terrainMapper = gameState.Island.Terrain_ForPower;
	}
	#endregion

	public bool CanBePlacedOn( SpaceState space ) => _inner.CanBePlacedOn( space, _terrainMapper );
	public bool IsSacredSite( Space space ) => _inner.IsSacredSite( _gameState.Tokens[space] );
	public IEnumerable<Space> Spaces => _inner.Spaces( _gameState ); // !!! Move everything that calls this over to SpaceStates, then remove this.
	public IEnumerable<SpaceState> SpaceStates => _inner.SpaceStates( _gameState );
	public IEnumerable<SpaceState> SacredSites => _inner.SacredSiteStates( _gameState, _terrainMapper );

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
		return options
			.Select(x=>x.Space); // ! TODO get reid of this line.
	}

	#region User Decisions

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	public async Task<IOption> SelectSource( string actionPhrase = "place" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await _self.Gateway.Decision( Select.TrackSlot.ToReveal( prompt, _self, _gameState ) )
			?? (IOption)await _self.Gateway.Decision( Select.DeployedPresence.All( prompt, this, Present.Always ) );
	}

	public Task<Space> SelectDeployed( string prompt )
		=> _self.Gateway.Decision( Select.DeployedPresence.All( prompt, this, Present.Always ) );

	public Task<Space> SelectSacredSite( string prompt )
		=> _self.Gateway.Decision( Select.DeployedPresence.SacredSites( prompt, this, Present.Always ) );

	/// <summary>Selects a Space within a range of spirits Presence</summary>
	/// <param name="targetingPowerType">
	/// None => standard ranging 
	/// Innate/PowerCard => power ranging  + Passes to PowerRanging
	/// </param>
	public async Task<Space> SelectDestinationWithinRange( TargetCriteria targetCriteria, TargetingPowerType targetingPowerType ) {
		var options = FindSpacesWithinRange( targetCriteria, targetingPowerType ).ToArray();
		return await _self.Gateway.Decision( Select.Space.ToPlacePresence( options, Present.Always, _inner.Token ) );
	}

	#endregion

	#region readonly fields
	readonly protected Spirit _self;
	readonly protected GameState _gameState;
	readonly protected TerrainMapper _terrainMapper;
	protected SpiritPresence _inner => _self.Presence;
	#endregion
}

/// <summary> High level Presence Methods for API </summary>
public class BoundPresence : ReadOnlyBoundPresence {

	#region constructor

	public BoundPresence(SelfCtx ctx):base(ctx) { 
		_actionId = ctx.CurrentActionId;
	}
	public BoundPresence( Spirit self, GameState gs, TerrainMapper terrainMapper, UnitOfWork actionId ) : base( self, gs, terrainMapper ) {
		_actionId = actionId;
	}

	readonly protected UnitOfWork _actionId;

	#endregion

	public Task Move( Space from, Space to ) => _inner.Move(from,to, _gameState, _actionId );
	public Task PlaceOn( Space space ) => _inner.PlaceOn( _gameState.Tokens[space], _actionId );
	public Task Destroy( Space space, DestoryPresenceCause actionType ) => _inner.Destroy( space, _gameState, actionType, _actionId );
	public Task RemoveFrom( Space space ) => _inner.RemoveFrom( space, _gameState ); // Generally used for Replacing, !!! should have an Action ID
	public Task Place( IOption from, Space to) => _inner.Place(from,to,_gameState,_actionId);


		// !!! should have an action ID
		public async Task<(Space, Space)> PushUpTo1() {
		// Select source
		var source = await _self.Gateway.Decision( Select.DeployedPresence.ToPush( this ) );
		if(source == null) return (null, null);

		// Select destination
		var destination = await _self.Gateway.Decision( Select.Space.PushPresence( source, _gameState.Tokens[source].Adjacent, Present.Always, _inner.Token ) );
		await Move( source, destination );
		return (source, destination);
	}

	/// <summary> Selects: (Source then Destination) for placing presence </summary>
	/// <remarks> Called from normal PlacePresence Growth + Gift of Proliferation. </remarks>
	/// !!! should Have an Action ID
	public async Task<(IOption,Space)> PlaceWithin( TargetCriteria targetCriteria, TargetingPowerType targetingPowerType ) {
		IOption from = await SelectSource();
		Space to = await SelectDestinationWithinRange( targetCriteria, targetingPowerType );
		await _self.Presence.Place( from, to, _gameState, _actionId );
		return(from, to);
	}

	/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
	/// <returns>Place in Ocean, Growth through sacrifice</returns>
	/// !!! should Have an Action ID
	public async Task Place( params Space[] destinationOptions ) {
		var from = await SelectSource();
		var to = await _self.Gateway.Decision( Select.Space.ToPlacePresence( destinationOptions, Present.Always, _inner.Token ) );
		await _self.Presence.Place( from, to, _gameState, _actionId );
	}

	/// !!! should Have an Action ID
	public async Task DestroyOneFromAnywhere( DestoryPresenceCause actionType, Func<SpiritIsland.SpaceState, bool> filter = null ) {
		var space = filter == null
			? await _self.Gateway.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", this ) )
			: await _self.Gateway.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", this, filter ) );
		await Destroy( space, actionType );
	}

	public async Task ReturnUpToNDestroyedToTrack( int count ) {
		count = Math.Max(count,_self.Presence.Destroyed);
		while(count > 0) {
			var dst = await _self.Gateway.Decision( Select.TrackSlot.ToCover( _self ) );
			if(dst == null) break;
			await _self.Presence.ReturnDestroyedToTrack(dst);
			--count;
		}
	}


}
