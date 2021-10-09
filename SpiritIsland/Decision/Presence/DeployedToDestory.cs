
using System.Collections.Generic;

namespace SpiritIsland.Decision.Presence {

	public class DeployedToDestory : Deployed {
		public DeployedToDestory(Spirit spirit):base( "Select presence to destroy", spirit ) { }
		public DeployedToDestory(string prompt, IEnumerable<Space> spaces, Present present ) : base( prompt, spaces, present ) { }
	}

}
