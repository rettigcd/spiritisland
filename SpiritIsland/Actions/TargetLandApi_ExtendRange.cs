using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	class TargetLandApi_ExtendRange : TargetLandApi {

		readonly int extension;
		readonly TargetLandApi originalApi;

		public TargetLandApi_ExtendRange( int extension, TargetLandApi originalApi ) {
			this.extension = extension;
			this.originalApi = originalApi;
		}

		public override IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum, PowerType powerType ) {
			return originalApi.GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range + extension, filterEnum, powerType );
		}

	}

}
