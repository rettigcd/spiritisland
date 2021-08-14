using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class GatherPresenceIntoOcean : GrowthActionFactory {

		public override async Task Activate( ActionEngine engine ) {
			List<Space> gatherSpaces = engine.Self.Presence.Placed
				.Where( p => p.IsCostal )
				.Select( p => p.Neighbors.Single( o => o.IsOcean ) )
				.Distinct()
				.ToList();

			while(0 < gatherSpaces.Count){

				Space currentTarget = gatherSpaces[0];
				Space source = await engine.SelectSpace(
					$"Select source of Presence to Gather into {currentTarget}"
					, currentTarget.Neighbors
						.Where( engine.Self.Presence.Placed.Contains )
						.ToArray()
				);

				// apply...
				engine.Self.Presence.Move(source,currentTarget);

				// next
				gatherSpaces.RemoveAt( 0 );

			} // while
		}

	}

}
