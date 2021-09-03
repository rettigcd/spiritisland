namespace SpiritIsland {

	public class SelectDeployedPresence : TypedDecision<Space> {
		public SelectDeployedPresence(string prompt, Spirit spirit):base(prompt, spirit.Presence.Spaces, Present.Always ) { }
	}

	public class SelectPresenceToDestory : SelectDeployedPresence {
		public SelectPresenceToDestory(Spirit spirit):base( "Select presence to destroy", spirit ) { }
	}

}
