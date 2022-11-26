namespace SpiritIsland;

public class ExploreEventArgs {

	public ExploreEventArgs( GameState gs, IEnumerable<SpaceState> sources, IEnumerable<SpaceState> spacesMatchingCards ) {
		this.Sources = new HashSet<SpaceState>( sources );
		this.SpacesMatchingCards = spacesMatchingCards.ToImmutableList();
		this.GameState = gs;
	}

	public GameState GameState { get; }

	/// <summary> Towns, cities, and coasts. </summary>
	public HashSet<SpaceState> Sources;

	/// <summary> Should be 2,3 or 4 per board.  (doesn't check sources) </summary>
	public ImmutableList<SpaceState> SpacesMatchingCards;

	public IEnumerable<SpaceState> Skipped => _skipped;

	// Add new spaces
	public void Add( Space space ) { // Pour time sideways
		throw new NotImplementedException();  // !!! 
	}

	public void Skip( SpaceState space ) {
		_skipped.Add( space );
	}
	public void SkipAll() {
		_skipped.AddRange(SpacesMatchingCards);
	}

	public IEnumerable<SpaceState> WillExplore( GameState _ ) {
		return ExploreRoutes
			.Where( rt => rt.IsValid )
			.Select( rt => rt.Destination )
			.Distinct()
			.OrderBy( x => x.Space.Label )
			.ToArray();
	}

	public IEnumerable<ExploreRoute> ExploreRoutes {
		get {
			return SpacesMatchingCards.Except(Skipped)
			.SelectMany( dst => dst.Range(1)
				.Where( Sources.Contains )
				.Select(src=>new ExploreRoute { Source = src, Destination = dst } )
			)
			.OrderBy(route => route.Destination.Space.Label)
			.ThenBy(route => route.Source.Space.Label)
			.ToArray();

		}
	}

	readonly List<SpaceState> _skipped = new List<SpaceState>();

}

public class ExploreRoute {
	public SpaceState Source;
	public SpaceState Destination;
	public bool IsValid { get {
		return Source == Destination
			|| Source[TokenType.Isolate] == 0
			&& Destination[TokenType.Isolate] == 0;
	} }
}
