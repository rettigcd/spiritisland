using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetLandApi {

		public async virtual Task<Space> TargetsSpace( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum ) {
			IEnumerable<Space> spaces = GetTargetOptions( self, sourceEnum, sourceTerrain, range, filterEnum, gameState );
			return await self.SelectSpace( "Select space to target.", spaces );
		}

		/// <remarks> Virtual so Entwined can override it </remarks>
		protected virtual IEnumerable<Space> GetTargetOptions( Spirit self, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum, GameState gameState ) {
			IEnumerable<Space> source = GetSource( self, sourceEnum );
			var debug = source.ToArray();
			if(sourceTerrain.HasValue)
				source = source.Where( x => x.Terrain == sourceTerrain.Value );

			IEnumerable<Space> spaces = source
				.Range( range )
				.Where( SpaceFilter.ForPowers.GetFilter( self, gameState, filterEnum ) );
			return spaces;
		}

		IEnumerable<Space> GetSource( Spirit self, From sourceEnum ) {
			return sourceEnum switch {
				From.Presence => GetPresenceSpaces( self ),
				From.SacredSite => GetSacredSites( self ),
				_ => throw new ArgumentException( "Invalid presence source " + sourceEnum ),
			};
		}

		protected virtual IEnumerable<Space> GetSacredSites( Spirit self ) {
			return self.SacredSites;
		}

		protected virtual IEnumerable<Space> GetPresenceSpaces( Spirit self ) {
			return self.Presence.Spaces;
		}
	}

	public enum From { None, Presence, SacredSite };

}
