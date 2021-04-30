using System;

namespace SpiritIsland {
	public class InvalidPresenceLocation : Exception {

		public InvalidPresenceLocation(string invalidSpace,string[] allowed)
			:base($"Invalid:{invalidSpace} allowed:"+string.Join(",",allowed))
		{}
		
	}

}
