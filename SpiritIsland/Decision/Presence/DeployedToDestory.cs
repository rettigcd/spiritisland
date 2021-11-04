﻿
using System.Collections.Generic;

namespace SpiritIsland.Decision.Presence {

	public class DeployedToDestory : Deployed {

		public DeployedToDestory(string prompt, Spirit spirit):base( prompt, spirit ) { }

		public DeployedToDestory(string prompt, IEnumerable<Space> spaces, Present present ) : base( prompt, spaces, present ) { }

	}

}
