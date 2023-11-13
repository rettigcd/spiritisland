namespace SpiritIsland;

/// <summary>
/// Configures TokenMover destination
/// </summary>
static public class Distribute {

	static public void Evenly( TokenMover mover ) {

		CountDictionary<Space> placed = new CountDictionary<Space>();
		mover
			.Track( theMove => placed[theMove.To.Space]++ )
			.FilterDestinationGroup( sss => {
				int min = sss.Sum( ss => placed[ss.Space] );
				return sss.Where( ss => placed[ss.Space] == min );
			} );
	}

	static public void ToAsManyLandsAsPossible( TokenMover mover ) {

		HashSet<SpaceState> used = new();
		mover.Track( theMove => used.Add( theMove.To ) )
			.FilterDestinationGroup( sss => {
				SpaceState[] unused = sss.Except( used ).ToArray();
				return 0 < unused.Length ? unused : sss;
			} );
	}

	/// <summary>
	/// When there are multiple destinations, user's first choice is only allowed choice.
	/// Not necessary when there is only 1 destination to start with.
	/// </summary>
	static public void ToASingleLand( TokenMover mover ) {
		Space destination = null;
		mover
			.Track( theMove => destination ??= theMove.To.Space )
			.FilterDestination( spaceState => destination is null || spaceState.Space == destination );
	}

}