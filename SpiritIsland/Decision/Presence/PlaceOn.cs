
using System.Collections.Generic;

namespace SpiritIsland.Decision.Presence {

	public class PlaceOn : TargetSpace {

		#region constructor
		public PlaceOn(Spirit spirit, IEnumerable<Space> destinationOptions, Present present )
			:base( "Where would you like to place your presence?", destinationOptions, present )
		{
			Spirit = spirit;
		}

		#endregion constructor

		public Spirit Spirit { get; }

	}


}