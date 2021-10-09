
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision.Presence {

	public class PlaceOn : TargetSpace {

		#region constructor
		public PlaceOn(Spirit spirit, IEnumerable<Space> destinationOptions, Present present )
			:base( "Where would you like to place your presence?", destinationOptions, present )
		{
			Spirit = spirit;
		}

		/// <summary>
		/// Allows users to select a space that is within [range] of their existing presence
		/// </summary>
		public PlaceOn(SpiritGameStateCtx ctx, int range, string filterEnum )
			:this( ctx.Self, FindSpacesWithinRangeOfSpiritsPresence(ctx,range,filterEnum ), Present.Always ) 
		{ 
		}

		#endregion constructor

		public Spirit Spirit { get; }

		/// <summary>
		/// Finds spaces within [range] of spirits existing presence
		/// </summary>
		static Space[] FindSpacesWithinRangeOfSpiritsPresence( SpiritGameStateCtx ctx, int range, string filterEnum ) {
			// Calculate options
			var existing = ctx.Self.Presence.Spaces.ToArray();

			var inRange = existing
				.SelectMany( s => s.Range( range ) )
				.Distinct()
				.ToArray();

			Space[] destinationOptions = inRange
				.Where( s=>ctx.Target(s).Matches(filterEnum) )
				.OrderBy( x => x.Label )
				.ToArray();

			return destinationOptions.Length == 0
				? throw new System.Exception( "dude you don't have anywhere to place your presence" )
				: destinationOptions;
		}

	}


}