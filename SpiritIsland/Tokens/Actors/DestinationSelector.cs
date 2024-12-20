namespace SpiritIsland;

/// <summary>
/// Selects destination for moving tokens.
/// Has dynamic filter if allowed destinations change.
/// </summary>
public sealed class DestinationSelector {

	static public DestinationSelector Adjacent => new DestinationSelector(GetAdjacents);
	static public DestinationSelector Nada => new DestinationSelector();
	static Space[] GetAdjacents(SpaceToken st) => st.Space.Adjacent.ToArray();

	#region constructors

	public DestinationSelector( params Space[] destinationSpaces ) {
		_findUnfilteredFromSource = _ => destinationSpaces;
		Single = destinationSpaces.Length == 1 ? destinationSpaces[0].SpaceSpec : null;
	}

	public DestinationSelector( IEnumerable<Space> destinationSpaces ):this(_=> destinationSpaces) {
		_findUnfilteredFromSource = ( _ ) => destinationSpaces.ToArray();
		var destArr = destinationSpaces.ToArray();
		Single = destArr.Length == 1 ? destArr[0].SpaceSpec : null;
	}

	public DestinationSelector( Func<SpaceToken, Space[]> getDestinationsFromSource ) {
		_findUnfilteredFromSource = getDestinationsFromSource;
		Single = null;
	}

	public DestinationSelector( Func<SpaceToken, IEnumerable<Space>> getDestinationsFromSource ) {
		_findUnfilteredFromSource = (x)=>getDestinationsFromSource(x).ToArray();
		Single = null;
	}

	#endregion

	#region public config

	public DestinationSelector Config(Action<DestinationSelector> configurer ) { configurer(this); return this; }

	/// <summary> Filters the spaces 1 at a time. </summary>
	public DestinationSelector FilterDestination( Func<Space, bool> filterDestination ) {
		_filterDestination = items => items.Where( filterDestination );
		return this;
	}

	/// <summary> Filters the spaces as a group. </summary>
	public DestinationSelector FilterDestinationGroup( Func<IEnumerable<Space>, IEnumerable<Space>> filterDestination ) {
		_filterDestination = filterDestination;
		return this;
	}

	public DestinationSelector ConfigAsOptional() {
		_present = Present.Done;
		return this;
	}

	#endregion public config

	public async Task<SpaceSpec?> SelectDestination( Spirit spirit, string actionWord, SpaceToken sourceSpaceToken ) {
		A.SpaceDecision decision = GetDecision( actionWord, sourceSpaceToken );
		Space? destination = await spirit.Select( decision );
		if(destination is not null)
			await NotifyAsync( destination );
		return destination?.SpaceSpec;
	}

	A.SpaceDecision GetDecision( string actionWord, SpaceToken sourceSpaceToken ) {
		Space[] options = GetDestinationOptions( sourceSpaceToken );
		A.SpaceDecision decision = new A.SpaceDecision( $"{actionWord} {sourceSpaceToken.Token.Text} to", options, _present )
			.ComingFrom( sourceSpaceToken.Space )
			.ShowTokenLocation( sourceSpaceToken.Token );
		return decision;
	}

	public Space[] GetDestinationOptions( SpaceToken sourceSpaceToken ) {
		return _filterDestination( _findUnfilteredFromSource( sourceSpaceToken ) )
			.Where( TerrainMapper.Current.IsInPlay )
			.ToArray();
	}

	public SpaceSpec? Single { get; }

	#region Event / Callback

	/// <summary> Tracking is only available for spaces. </summary>
	public DestinationSelector Track( Action<Space> onDestinationSelected ) {
		_onMoved.Add( (x)=>{ if(x is Space space) onDestinationSelected(space); return Task.CompletedTask; } );
		return this;
	}

	public DestinationSelector Track( Func<ILocation,Task> onDestinationSelected ) {
		_onMoved.Add(onDestinationSelected);
		return this;
	}

	public async Task NotifyAsync( ILocation destination ) {
		foreach(var onMoved in _onMoved)
			await onMoved( destination );
	}

	readonly List<Func<ILocation, Task>> _onMoved = [];

	#endregion

	Func<IEnumerable<Space>, IEnumerable<Space>> _filterDestination = x => x;

	readonly Func<SpaceToken, Space[]> _findUnfilteredFromSource;
	Present _present = Present.AutoSelectSingle;

}
