using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetLandApi {

		public async virtual Task<Space> TargetsSpace( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			IEnumerable<Space> spaces = GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range, filterEnum );
			return await self.Action.Decision( new Decision.TargetSpace( "Select space to target.", spaces ));
		}

		public virtual IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			IEnumerable<Space> source = FindSources( self, sourceEnum, sourceTerrain );
			return GetTargetOptions( self, gameState, source, range, filterEnum );
		}

		protected IEnumerable<Space> FindSources( Spirit self, From sourceEnum, Terrain? sourceTerrain ) {
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
				.SelectMany( x => x.Range( range ) )
				.Distinct()
				.Where( SpaceFilter.ForPowers.GetFilter( self, gameState, filterEnum ) ); // matching this destination
		}

		static IEnumerable<Space> GetSource( Spirit self, From sourceEnum ) {
			return sourceEnum switch {
				From.Presence => self.Presence.Spaces,
				From.SacredSite => self.SacredSites,
				_ => throw new ArgumentException( "Invalid presence source " + sourceEnum ),
			};
		}

		#region Static Public

		static public void ScheduleRestore( SpiritGameStateCtx ctx ) {
			ctx.GameState.TimePasses_ThisRound.Push( new PowerApiRestorer( ctx.Self ).Restore );
		}

		static public void ExtendRange( Spirit spirit, int rangeExtension ) {
			spirit.PowerApi = new TargetLandApi_ExtendRange( rangeExtension, spirit.PowerApi );
		}

		#endregion

	}

	public enum From { None, Presence, SacredSite };

}
