using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class GatherPresenceIntoOcean : GrowthActionFactory {

		public override async Task Activate( ActionEngine engine ) {
			List<Space> gatherSpaces = engine.Self.Presence.Spaces
				.Where( p => p.IsCostal )
				.Select( p => p.Adjacent.Single( o => o.IsOcean ) )
				.Distinct()
				.ToList();

			while(0 < gatherSpaces.Count){

				Space currentTarget = gatherSpaces[0];
				Space source = await engine.SelectSpace(
					$"Select source of Presence to Gather into {currentTarget}"
					, currentTarget.Adjacent
						.Where( engine.Self.Presence.Spaces.Contains )
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
