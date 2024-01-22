namespace SpiritIsland;

/// <summary>
/// Configures TokenMover destination
/// </summary>
static public class Distribute {

	static public void Evenly( DestinationSelector d ) {
		CountDictionary<Space> placed = [];
		d.Track( to => placed[to.Space]++ );
		d.FilterDestinationGroup( sss => {
			int min = sss.Sum( ss => placed[ss.Space] );
			return sss.Where( ss => placed[ss.Space] == min );
		});
	}

	static public void ToAsManyLandsAsPossible( DestinationSelector d ) {

		HashSet<SpaceState> used = [];
		d.Track( to => used.Add( to ) );
		d.FilterDestinationGroup( sss => {
			SpaceState[] unused = sss.Except( used ).ToArray();
			return 0 < unused.Length ? unused : sss;
		});
	}

	/// <summary>
	/// When there are multiple destinations, user's first choice is only allowed choice.
	/// Not necessary when there is only 1 destination to start with.
	/// </summary>
	static public void ToASingleLand( DestinationSelector d ) {
		Space destination = null;
		d.Track( to => destination ??= to.Space );
		d.FilterDestination( spaceState => destination is null || spaceState.Space == destination );
	}

	/// <summary>
	/// Performs the action once per destination land.
	/// </summary>
	static public Action<DestinationSelector> OnEachDestinationLand( Action<SpaceState> pushedAction ) {
		return (d) => {
			var pushedToLands = new HashSet<SpaceState>();
			d.Track(to => {
				if(pushedToLands.Contains(to)) return;
				pushedToLands.Add(to);
				pushedAction(to);
			} );
		};
	}

	static public Action<DestinationSelector> OnEachDestinationLand( Func<SpaceState,Task> pushedActionAsync ) {
		return (d) => {
			var pushedToLands = new HashSet<SpaceState>();
			d.Track(async to => {
				if(pushedToLands.Contains(to)) return;
				pushedToLands.Add(to);
				await pushedActionAsync(to);
			} );
		};
	}

}