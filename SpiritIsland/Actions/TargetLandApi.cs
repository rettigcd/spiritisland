using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TargetLandApi {

		public async virtual Task<Space> TargetsSpace( Spirit self, GameState gameState, string prompt, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			if(prompt == null) prompt = "Target Space.";
			IEnumerable<Space> spaces = GetTargetOptions( self, gameState, sourceEnum, sourceTerrain, range, filterEnum );
			return await self.Action.Decision( new Decision.TargetSpace( prompt, spaces, Present.Always ));
		}

		public virtual IEnumerable<Space> GetTargetOptions( Spirit self, GameState gameState, From sourceEnum, Terrain? sourceTerrain, int range, string filterEnum ) {
			IEnumerable<Space> source = FindSources( self, sourceEnum, sourceTerrain );
			return GetTargetOptions( self, gameState, source, range, filterEnum );
		}

		static protected IEnumerable<Space> FindSources( Spirit self, From sourceEnum, Terrain? sourceTerrain ) {
			// Select Source
			IEnumerable<Space> source = GetSource( self, sourceEnum );
			if(sourceTerrain.HasValue)
				source = source.Where( x => x.Terrain == sourceTerrain.Value ); // filter source
			return source;
		}

#pragma warning disable CA1822 // Mark members as static
		public IEnumerable<Space> GetTargetOptions( 
#pragma warning restore CA1822 // Mark members as static
			Spirit self, 
			GameState gameState, 
			IEnumerable<Space> source, 
			int range, 
			string filterEnum 
		) {
			var ctx = new SpiritGameStateCtx( self, gameState, Cause.Power );
			return source       // starting here
				.SelectMany( x => x.Range( range ) )
				.Distinct()
				.Where( s => ctx.Target(s).Matches( filterEnum ) ); // matching this destination
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

		/// <remarks>Pair this with ScheduleResotre</remarks>
		static public void ExtendRange( Spirit spirit, int rangeExtension ) {
			spirit.TargetLandApi = new TargetLandApi_ExtendRange( rangeExtension, spirit.TargetLandApi );
		}

		#endregion

	}

	public enum From { None, Presence, SacredSite };

}
