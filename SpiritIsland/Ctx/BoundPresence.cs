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

	// !!! Note - Some of these are pass-thru and ignore GameState and possibly could be simplified.
	// This class is all about binding presence to gamestate so stuff that ignores gamestate maybe doesn't go here.

	public bool CanBePlacedOn( SpaceState space ) => _inner.CanBePlacedOn( space, _terrainMapper );
	public bool IsSacredSite( Space space ) => _inner.IsSacredSite( _gameState.Tokens[space] );

	public IEnumerable<SpaceState> ActiveSpaceStates => _inner.ActiveSpaceStates( _gameState );
	public IEnumerable<SpaceState> MovableSpaceStates => _inner.ActiveSpaceStates( _gameState ).Where(_inner.HasMovableTokens);
	public IEnumerable<SpaceState> SacredSites => _inner.SacredSiteStates( _gameState, _terrainMapper );

	// This is the non-Targetting version that skips over the TargetSourceCalc
	public IEnumerable<SpaceState> FindSpacesWithinRange( TargetCriteria targetCriteria, bool forPower )
		=> _self.FindSpacesWithinRange( _gameState, targetCriteria, forPower ); // !! Only being used with Power since we are assuming the Terrain Mapper

	public IVisibleToken Token => _inner.Token;

	#region User Decisions

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	public async Task<IOption> SelectSource( string actionPhrase = "place" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await _self.Gateway.Decision( Select.TrackSlot.ToReveal( prompt, _self, _gameState ) )
			?? (IOption)await _self.Gateway.Decision( Select.DeployedPresence.All( prompt, this, Present.Always ) );
	}

	/// <summary> Tries Presence Tracks first, then fails over to placed-presence on Island </summary>
	public async Task<IOption> SelectSource_Movable( string actionPhrase = "move" ) {
		string prompt = $"Select Presence to {actionPhrase}";
		return (IOption)await _self.Gateway.Decision( Select.TrackSlot.ToReveal( prompt, _self, _gameState ) )
			?? (IOption)await _self.Gateway.Decision( Select.DeployedPresence.Movable( prompt, this, Present.Always ) );
	}

	public Task<Space> SelectDeployed( string prompt )
		=> _self.Gateway.Decision( Select.DeployedPresence.All( prompt, this, Present.Always ) );
	public Task<Space> SelectDeployedMovable( string prompt )
		=> _self.Gateway.Decision( Select.DeployedPresence.Movable( prompt, this, Present.Always ) );

	public Task<Space> SelectSacredSite( string prompt )
		=> _self.Gateway.Decision( Select.DeployedPresence.SacredSites( prompt, this, Present.Always ) );

	/// <summary>Selects a Space within a range of spirits Presence</summary>
	/// <param name="targetingPowerType">
	/// None => standard ranging 
	/// Innate/PowerCard => power ranging  + Passes to PowerRanging
	/// </param>
	public async Task<Space> SelectDestinationWithinRange( TargetCriteria targetCriteria, bool forPower ) {
		var options = FindSpacesWithinRange( targetCriteria, forPower )
			.Where( CanBePlacedOn )
			.ToArray();
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
	}

	public BoundPresence( Spirit self, GameState gs, TerrainMapper terrainMapper ) : base( self, gs, terrainMapper ) {}

	#endregion

	public async Task Move( Space from, Space to ) { 
		await _gameState.Tokens[from].BindScope().MoveTo(_inner.Token, to );
	}
	public Task PlaceOn( Space space ) => _gameState.Tokens[space].BindScope().Add( Token, 1 );
	public Task Destroy( Space space, int count, DestoryPresenceCause _ ) => _gameState.Tokens[space].BindScope().Destroy( _inner.Token, count );
	public Task RemoveFrom( Space space ) {
		return _gameState.Tokens[space].BindScope().Remove( Token, 1, RemoveReason.Removed );
	}
	public Task Place( IOption from, Space to) => _inner.Place(from,to,_gameState);

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
	public async Task<(IOption,Space)> PlaceWithin( TargetCriteria targetCriteria, bool forPower ) {
		IOption from = await SelectSource();
		Space to = await SelectDestinationWithinRange( targetCriteria, forPower );
		await _self.Presence.Place( from, to, _gameState );
		return(from, to);
	}

	/// <summary> Selects: Source then Destination(predetermined) for placing presence.</summary>
	/// <returns>Place in Ocean, Growth through sacrifice</returns>
	/// !!! should Have an Action ID
	public async Task Place( params Space[] destinationOptions ) {

		IOption from = await SelectSource();
		Space to = await _self.Gateway.Decision( Select.Space.ToPlacePresence( _gameState.Tokens.PowerUp( destinationOptions ), Present.Always, _inner.Token ) );
		await _self.Presence.Place( from, to, _gameState );
	}

	/// !!! should Have an Action ID
	public async Task DestroyOneFromAnywhere( DestoryPresenceCause actionType, Func<SpiritIsland.SpaceState, bool> filter = null ) {
		var space = filter == null
			? await _self.Gateway.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", this ) )
			: await _self.Gateway.Decision( Select.DeployedPresence.ToDestroy( "Select presence to destroy", this, filter ) );
		await Destroy( space, 1, actionType );
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
