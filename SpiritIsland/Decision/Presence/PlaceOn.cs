
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision.Presence {

	public class PlaceOn : TargetSpace {

		#region constructor
		public PlaceOn(Spirit spirit, IEnumerable<Space> destinationOptions )
			:base( "Where would you like to place your presence?", destinationOptions, Present.Always )
		{
			Spirit = spirit;
		}
		public PlaceOn(SpiritGameStateCtx ctx, int range, string filterEnum )
			:this( ctx.Self, CalcDestinationOptions(ctx,range,filterEnum ) ) 
		{ 
		}

		#endregion constructor

		public Spirit Spirit { get; }

		static Space[] CalcDestinationOptions( SpiritGameStateCtx ctx, int range, string filterEnum ) {
			// Calculate options
			var existing = ctx.Self.Presence.Spaces.ToArray();

			var inRange = existing
				.SelectMany( s => s.Range( range ) )
				.Distinct()
				.ToArray();

			Space[] destinationOptions = inRange
				.Where( SpaceFilter.Normal.GetFilter( ctx.Self, ctx.GameState, filterEnum ) )
				.OrderBy( x => x.Label )
				.ToArray();

			return destinationOptions.Length == 0
				? throw new System.Exception( "dude you don't have anywhere to place your presence" )
				: destinationOptions;
		}

	}


}