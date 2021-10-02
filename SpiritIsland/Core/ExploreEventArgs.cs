using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SpiritIsland {
	public class ExploreEventArgs {

		public ExploreEventArgs(HashSet<Space> sources,List<Space> spacesMatchingCards ) {
			this.Sources = sources;
			this.SpacesMatchingCards = spacesMatchingCards.ToImmutableList();
		}

		/// <summary> Towns, cities, and coasts. </summary>
		public HashSet<Space> Sources;

		/// <summary> Should be 2,3 or 4 per board.  (doesn't check sources) </summary>
		public ImmutableList<Space> SpacesMatchingCards;

		public IEnumerable<Space> Skipped => _skipped;

		public void Skip( Space space ) {
			_skipped.Add( space );
		}
		public void SkipAll() {
			_skipped.AddRange(SpacesMatchingCards);
		}

			// Add new spaces
		public IEnumerable<Space> WillExplore => SpacesMatchingCards.Except(Skipped)
			.OrderBy(x=>x.Label)
			.Where( space => space.Range( 1 ).Any( Sources.Contains ) )
			.ToArray();


		readonly List<Space> _skipped = new List<Space>();

	}

}
