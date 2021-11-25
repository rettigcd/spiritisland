using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	#region Source
	public enum From { None, Presence, SacredSite };

	public interface ICalcSource {
		IEnumerable<Space> FindSources( Spirit self, From sourceEnum, Terrain? sourceTerrain );

	}

	public class DefaultSourceCalc : ICalcSource {
		// !! ??? does this need to be virtual?  is anything overriding it and calling base.FindSources() ???
		public virtual IEnumerable<Space> FindSources( Spirit self, From sourceEnum, Terrain? sourceTerrain ) {
			var sources = sourceEnum switch {
				From.Presence => self.Presence.Spaces,
				From.SacredSite => self.SacredSites,
				_ => throw new ArgumentException( "Invalid presence source " + sourceEnum ),
			};
			return sources.Where( x => !sourceTerrain.HasValue || sourceTerrain.Value == x.Terrain );
		}
	}

	#endregion

	#region Range

	public interface ICalcRange {
		IEnumerable<Space> GetTargetOptionsFromKnownSource( 
			Spirit self, 
			GameState gameState, 
			int range, 
			string filterEnum,
			TargettingFrom powerType,
			IEnumerable<Space> source
		);

	}

	public class DefaultCalcRange : ICalcRange {

		// Find Range
		// This is virtual so Volcano Targetting can call base()
		public virtual IEnumerable<Space> GetTargetOptionsFromKnownSource( 
			Spirit self, 
			GameState gameState, 
			int range, 
			string filterEnum,
			TargettingFrom powerType,
			IEnumerable<Space> source
		) {
			var ctx = new SpiritGameStateCtx( self, gameState, Cause.Power );
			return source       // starting here
				.SelectMany( x => x.Range( range ) )
				.Distinct()
				.Where( s => ctx.Target(s).Matches( filterEnum ) ); // matching this destination
		}

	}

	#endregion

}
