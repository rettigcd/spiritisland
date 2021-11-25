using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.JaggedEarth {

	class VolcanoTargetLandApi : DefaultCalcRange {

		public override IEnumerable<Space> GetTargetOptionsFromKnownSource( Spirit self, GameState gameState, int range, string filterEnum, TargettingFrom powerType, IEnumerable<Space> source ) {
			List<Space> spaces = base.GetTargetOptionsFromKnownSource( self, gameState, range, filterEnum, powerType, source )
				.ToList();

			// Add towers
			if(powerType != TargettingFrom.Innate) {
				var towers = self.Presence.Placed.Where(s=>3 <= self.Presence.CountOn(s)).ToArray();
				if(towers.Length>0)
					spaces.AddRange( base.GetTargetOptionsFromKnownSource(self,gameState,range+1,filterEnum,powerType,towers) );
			}

			return spaces.Distinct();
		}

	}


}
