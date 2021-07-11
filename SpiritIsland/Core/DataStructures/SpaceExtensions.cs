using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	static public class SpaceExtensions {

		static public IEnumerable<Space> Range(this IEnumerable<Space> source, int distance)
			=> source
					.SelectMany(x => x.SpacesWithin(distance))
					.Distinct();

	}
}
