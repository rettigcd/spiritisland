using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetLandApi {

		public async virtual Task<Space> TargetsSpace( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			IEnumerable<Space> spaces = GetTargetOptions( self, sourceEnum, sourceTerrain, range, filterEnum, gameState );
			return await self.Action.Decide( new TargetSpaceDecision( "Select space to target.", spaces ));
		}

		protected virtual IEnumerable<Space> GetTargetOptions( Spirit self, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum, GameState gameState ) {
			IEnumerable<Space> source = FindSources( self, sourceEnum, sourceTerrain );
			return GetTargetOptions( self, gameState, source, range, filterEnum );
		}

		/// <remarks> Virtual so Entwined can override it </remarks>
		public virtual IEnumerable<Space> FindSources( Spirit self, From sourceEnum, Terrain? sourceTerrain ) {
			// Select Source
			IEnumerable<Space> source = GetSource( self, sourceEnum );
			if(sourceTerrain.HasValue)
				source = source.Where( x => x.Terrain == sourceTerrain.Value ); // filter source
			return source;
		}

		public IEnumerable<Space> GetTargetOptions( 
			Spirit self, 
			GameState gameState, 
			IEnumerable<Space> source, 
			int range, 
			string filterEnum 
		) {
			return source       // starting here
				.Range( range ) // find spaces within range
				.Where( SpaceFilter.ForPowers.GetFilter( self, gameState, filterEnum ) ); // matching this destination
		}

		static IEnumerable<Space> GetSource( Spirit self, From sourceEnum ) {
			return sourceEnum switch {
				From.Presence => self.Presence.Spaces,
				From.SacredSite => self.SacredSites,
				_ => throw new ArgumentException( "Invalid presence source " + sourceEnum ),
			};
		}

	}

	public enum From { None, Presence, SacredSite };

}
