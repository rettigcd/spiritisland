using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetLandApi {

		public async virtual Task<Space> TargetsSpace( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum ) {
			IEnumerable<Space> spaces = GetTargetOptions( self, sourceEnum, sourceTerrain, range, filterEnum, gameState );
			return await self.Action.Decide( new TargetSpaceDecision( "Select space to target.", spaces ));
		}

		/// <remarks> Virtual so Entwined can override it </remarks>
		protected virtual IEnumerable<Space> GetTargetOptions( Spirit self, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum, GameState gameState ) {
			// Select Source
			IEnumerable<Space> source = GetSource( self, sourceEnum );
			if(sourceTerrain.HasValue)
				source = source.Where( x => x.Terrain == sourceTerrain.Value ); // filter source

			return source		// starting here
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
