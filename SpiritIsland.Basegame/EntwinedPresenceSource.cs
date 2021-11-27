using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class EntwinedPresenceSource : ICalcSource {

		readonly Spirit[] spirits;
		readonly ICalcSource[] origApis;

		public EntwinedPresenceSource( params Spirit[] spirits ) {
			this.spirits = spirits;
			this.origApis = spirits.Select(x=>x.SourceCalc).ToArray();
			
			foreach(var spirit in spirits)
				spirit.SourceCalc = this;
		}

		public IEnumerable<Space> FindSources( IKnowSpiritLocations _, From sourceEnum, Terrain? sourceTerrain ) {
			List<Space> sources = new ();
			// Find source of original
			for(int i = 0; i < spirits.Length; ++i)
				sources.AddRange( origApis[i].FindSources( spirits[i].Presence, sourceEnum, sourceTerrain ) );
			return sources.Distinct();
		}

	}

}

