
namespace SpiritIsland.Decision {

	public class PresenceDeployed : TypedDecision<Space> {
		public PresenceDeployed(string prompt, Spirit spirit):base(prompt, spirit.Presence.Spaces, Present.Always ) { }
	}

}
