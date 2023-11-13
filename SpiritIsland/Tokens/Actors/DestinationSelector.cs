namespace SpiritIsland;

/// <summary>
/// Selects destination for moving tokens.
/// Has dynamic filter if allowed destinations change.
/// </summary>
public class DestinationSelector {

	static public DestinationSelector Adjacent => new DestinationSelector(GetAdjacents);
	static public DestinationSelector Nada => new DestinationSelector();
	static SpaceState[] GetAdjacents(SpaceToken st) => st.Space.Tokens.Adjacent.ToArray();

	#region constructors

	public DestinationSelector( params SpaceState[] destinationSpaces ) {
		_unfiltered = _ => destinationSpaces;
		Single = destinationSpaces.Length == 1 ? destinationSpaces[0].Space : null;
	}

	public DestinationSelector( IEnumerable<SpaceState> destinationSpaces ):this(_=> destinationSpaces) {
		_unfiltered = ( _ ) => destinationSpaces.ToArray();
		var destArr = destinationSpaces.ToArray();
		Single = destArr.Length == 1 ? destArr[0].Space : null;
	}


	public DestinationSelector( Func<SpaceToken, SpaceState[]> getDestinationsFromSource ) {
		_unfiltered = getDestinationsFromSource;
		Single = null;
	}

	public DestinationSelector( Func<SpaceToken, IEnumerable<SpaceState>> getDestinationsFromSource ) {
		_unfiltered = (x)=>getDestinationsFromSource(x).ToArray();
		Single = null;
	}

	#endregion

	#region public config

	/// <summary> Filters the spaces 1 at a time. </summary>
	public DestinationSelector FilterDestination( Func<SpaceState, bool> filterDestination ) {
		_filterDestination = items => items.Where( filterDestination );
		return this;
	}

	/// <summary> Filters the spaces as a group. </summary>
	public DestinationSelector FilterDestinationGroup( Func<IEnumerable<SpaceState>, IEnumerable<SpaceState>> filterDestination ) {
		_filterDestination = filterDestination;
		return this;
	}


	#endregion public config

	public virtual async Task<Space> SelectDestination( Spirit spirit, string actionWord, SpaceToken sourceSpaceToken ) {

		var unfiltered = _unfiltered( sourceSpaceToken ).ToArray();
		var filtered = _filterDestination( unfiltered ).ToArray();
		var inPlay = filtered.Where( ss => TerrainMapper.Current.IsInPlay( ss.Space ) ).ToArray();


		IEnumerable<SpaceState> destinationOptions = _filterDestination( _unfiltered( sourceSpaceToken ) )
			.Where(ss=>TerrainMapper.Current.IsInPlay(ss.Space));

		return await spirit.Select(
			new A.Space( $"{actionWord} {sourceSpaceToken.Token.Text} to", inPlay.Downgrade(), Present.AutoSelectSingle )
				.ComingFrom( sourceSpaceToken.Space )
				.ShowTokenLocation( sourceSpaceToken.Token )
			);
	}

	public Space Single { get; }

	Func<IEnumerable<SpaceState>, IEnumerable<SpaceState>> _filterDestination = x => x;

	readonly protected Func<SpaceToken, SpaceState[]> _unfiltered;

}
