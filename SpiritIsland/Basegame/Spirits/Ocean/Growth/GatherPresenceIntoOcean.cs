using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class GatherPresenceIntoOcean : GrowthActionFactory {

		public override async Task ActivateAsync( Spirit self, GameState _ ) {
			List<Space> gatherSpaces = self.Presence.Spaces
				.Where( p => p.IsCostal )
				.Select( p => p.Adjacent.Single( o => o.Terrain == Terrain.Ocean ) )
				.Distinct()
				.ToList();

			while(0 < gatherSpaces.Count){

				Space currentTarget = gatherSpaces[0];
				Space source = await self.Action.Choose( new TargetSpaceDecision(
					$"Select source of Presence to Gather into {currentTarget}"
					, currentTarget.Adjacent
						.Where( self.Presence.Spaces.Contains )
						.ToArray()
				));

				// apply...
				self.Presence.Move(source,currentTarget);

				// next
				gatherSpaces.RemoveAt( 0 );

			} // while
		}

	}

}
