namespace SpiritIsland;

/// <summary>
/// Configures TokenMover destination
/// </summary>
static public class Distribute {

	static public void Evenly( DestinationSelector d ) {
		CountDictionary<Space> placed = [];
		d.Track( to => placed[to]++ );
		d.FilterDestinationGroup( sss => {
			int min = sss.Sum( ss => placed[ss] );
			return sss.Where( ss => placed[ss] == min );
		});
	}

	static public void ToAsManyLandsAsPossible( DestinationSelector d ) {

		HashSet<Space> used = [];
		d.Track( to => used.Add( to ) );
		d.FilterDestinationGroup( sss => {
			Space[] unused = sss.Except( used ).ToArray();
			return 0 < unused.Length ? unused : sss;
		});
	}

	/// <summary>
	/// When there are multiple destinations, user's first choice is only allowed choice.
	/// Not necessary when there is only 1 destination to start with.
	/// </summary>
	static public void ToASingleLand( DestinationSelector d ) {
		Space? destination = null;
		d.Track( to => destination ??= to );
		d.FilterDestination( space => destination is null || space == destination );
	}

	/// <summary>
	/// Performs the action once per destination land.
	/// </summary>
	static public Action<DestinationSelector> OnEachDestinationLand( Action<Space> pushedAction ) {
		return (d) => {
			var pushedToLands = new HashSet<Space>();
			d.Track(to => {
				if(pushedToLands.Contains(to)) return;
				pushedToLands.Add(to);
				pushedAction(to);
			} );
		};
	}

	static public Action<DestinationSelector> OnEachDestinationLand( Func<Space,Task> pushedActionAsync ) {
		return (d) => {
			var pushedToLands = new HashSet<Space>();
			d.Track(async to => {
				if(pushedToLands.Contains(to)) return;
				pushedToLands.Add((Space)to);
				await pushedActionAsync((Space)to);
			} );
		};
	}

}