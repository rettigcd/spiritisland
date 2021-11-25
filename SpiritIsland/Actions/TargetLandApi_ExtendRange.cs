using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetLandApi_ExtendRange : ICalcRange {

		readonly int extension;
		readonly ICalcRange originalApi;

		public TargetLandApi_ExtendRange( int extension, ICalcRange originalApi ) {
			this.extension = extension;
			this.originalApi = originalApi;
		}

		public IEnumerable<Space> GetTargetOptionsFromKnownSource( Spirit self, GameState gameState, int range, string filterEnum, TargettingFrom powerType, IEnumerable<Space> source ) {
			return originalApi.GetTargetOptionsFromKnownSource( self, gameState, range + extension, filterEnum, powerType, source );
		}

	}

}
