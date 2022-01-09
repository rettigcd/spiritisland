using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SpiritIsland {

	public class ExploreEventArgs {

		public ExploreEventArgs( GameState gs, IEnumerable<Space> sources, IEnumerable<Space> spacesMatchingCards ) {
			this.Sources = new HashSet<Space>( sources );
			this.SpacesMatchingCards = spacesMatchingCards.ToImmutableList();
			this.GameState = gs;
		}

		public GameState GameState { get; }

		/// <summary> Towns, cities, and coasts. </summary>
		public HashSet<Space> Sources;

		/// <summary> Should be 2,3 or 4 per board.  (doesn't check sources) </summary>
		public ImmutableList<Space> SpacesMatchingCards;

		public IEnumerable<Space> Skipped => _skipped;

		// Add new spaces
		public void Add( Space space ) { // Pour time sideways
			throw new NotImplementedException();  // !!! 
		}

		public void Skip( Space space ) {
			_skipped.Add( space );
		}
		public void SkipAll() {
			_skipped.AddRange(SpacesMatchingCards);
		}

		public IEnumerable<Space> WillExplore( GameState gs ) {
			return ExploreRoutes
				.Where( rt => rt.IsValid( gs ) )
				.Select( rt => rt.Destination )
				.Distinct()
				.OrderBy( x => x.Label )
				.ToArray();
		}

		public IEnumerable<ExploreRoute> ExploreRoutes {
			get {
				return SpacesMatchingCards.Except(Skipped)
				.SelectMany( dst => dst.Range(1)
					.Where( Sources.Contains )
					.Select(src=>new ExploreRoute { Source = src, Destination = dst } )
				)
				.OrderBy(route => route.Destination.Label)
				.ThenBy(route => route.Source.Label)
				.ToArray();

			}
		}

		readonly List<Space> _skipped = new List<Space>();

	}

	public class ExploreRoute {
		public Space Source;
		public Space Destination;
		public bool IsValid( GameState gs ) {
			return Source == Destination
				|| gs.Tokens[Source][TokenType.Isolate] == 0
				&& gs.Tokens[Destination][TokenType.Isolate] == 0;
		}
	}

}
