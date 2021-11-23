using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class SharedPresenceTargeting : TargetLandApi {

		readonly Spirit[] spirits;
		readonly TargetLandApi[] origApis;

		public SharedPresenceTargeting( params Spirit[] spirits ) {
			this.spirits = spirits;
			this.origApis = spirits.Select(x=>x.TargetLandApi).ToArray();
			
			foreach(var spirit in spirits)
				spirit.TargetLandApi = this;
		}

		public override IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			List<Space> options = new ();
			for(int i = 0; i < spirits.Length; ++i)
				options.AddRange( origApis[i].GetTargetOptions( spirits[i], gameState, sourceEnum, sourceTerrain, range, filterEnum ) );
			return options.Distinct();
		}

	}

}

