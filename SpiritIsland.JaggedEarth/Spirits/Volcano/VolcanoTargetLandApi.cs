using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.JaggedEarth {

	class VolcanoTargetLandApi : TargetLandApi {

		public override IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum, PowerType powerType ) {

			List<Space> spaces = base.GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range, filterEnum, powerType )
				.ToList();

			// Add towers
			if(powerType != PowerType.Innate) {
				var towers = self.Presence.Placed.Where(s=>3 <= self.Presence.CountOn(s)).ToArray();
				if(towers.Length>0)
					spaces.AddRange( base.GetTargetOptionsFromKnownSource(self,gameState,towers,range+1,filterEnum) );
			}

			return spaces.Distinct();
		}

	}


}
