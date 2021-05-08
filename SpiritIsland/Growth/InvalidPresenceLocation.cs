using System;
using System.Linq;

namespace SpiritIsland {
	public class InvalidPresenceLocation : Exception {

		public InvalidPresenceLocation(string invalidSpace,string[] allowed)
			:base($"Invalid:{invalidSpace} allowed:"+allowed.Join(","))
		{}
		
	}

}
