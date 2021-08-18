using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetLandApi {

		#region constructor

		public TargetLandApi(){
		}

		#endregion

		public Task<Space> TargetSpace( ActionEngine engine, From sourceEnum, int range, Target filterEnum )
			=> TargetSpace( engine, sourceEnum, null, range, filterEnum );

		public async virtual Task<Space> TargetSpace( ActionEngine engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum ) {
			IEnumerable<Space> spaces = GetTargetOptions( engine.Self, sourceEnum, sourceTerrain, range, filterEnum, engine.GameState );
			return await engine.Self.SelectSpace( "Select target.", spaces );
		}

		public virtual IEnumerable<Space> GetTargetOptions( Spirit self, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum, GameState gameState ) {
			var source = sourceEnum switch {
				From.Presence => self.Presence.Spaces,
				From.SacredSite => self.SacredSites,
				_ => throw new ArgumentException( "Invalid presence source " + sourceEnum ),
			};

			if(sourceTerrain.HasValue)
				source = source.Where(x=>x.Terrain == sourceTerrain.Value);

			IEnumerable<Space> spaces = source
				.Range( range )
				.Where( TargetSpaceAttribute.ToLambda( self, gameState, filterEnum ) );
			return spaces;
		}

	}

	public enum From { None, Presence, SacredSite };

}
