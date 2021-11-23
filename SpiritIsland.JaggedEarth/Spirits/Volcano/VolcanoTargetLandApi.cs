using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	class VolcanoTargetLandApi : TargetLandApi {

		public override IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {

			List<Space> spaces = base.GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range, filterEnum )
				.ToList();

			// Add towns
			var towers = self.Presence.Placed.Where(s=>3 <= self.Presence.CountOn(s)).ToArray();
			spaces.AddRange( base.GetTargetOptionsFromKnownSource(self,gameState,towers,range+1,filterEnum) );

			return spaces.Distinct();
		}

	}


}
