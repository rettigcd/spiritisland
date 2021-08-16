using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class PowerCardApi {

		#region constructor

		public PowerCardApi(){
		}

		#endregion

		#region virutal Target

		public virtual IEnumerable<Space> GetTargetOptions( ActionEngine engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum ) {
			var source = sourceEnum switch {
				From.Presence => engine.Self.Presence.Spaces,
				From.SacredSite => engine.Self.SacredSites,
				_ => throw new ArgumentException( "Invalid presence source " + sourceEnum ),
			};

			if(sourceTerrain.HasValue)
				source = source.Where(x=>x.Terrain == sourceTerrain.Value);

			IEnumerable<Space> spaces = source
				.Range( range )
				.Where( TargetSpaceAttribute.ToLambda( engine, filterEnum ) );
			return spaces;
		}

		public virtual async Task<Space> TargetSpace( ActionEngine engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum ) {
			IEnumerable<Space> spaces = GetTargetOptions( engine, sourceEnum, sourceTerrain, range, filterEnum );
			return await engine.SelectSpace( "Select target.", spaces );
		}

		public Task<Space> TargetSpace( ActionEngine engine, From sourceEnum, int range, Target filterEnum )
			=> TargetSpace(engine,sourceEnum,null,range,filterEnum);
	
		public IEnumerable<Space> GetTargetOptions( ActionEngine engine, From sourceEnum, int range, Target filterEnum )
			=> GetTargetOptions( engine, sourceEnum, null, range, filterEnum );

		#endregion

	}

	public enum From { None, Presence, SacredSite };

}
