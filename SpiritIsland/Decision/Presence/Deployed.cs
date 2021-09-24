
namespace SpiritIsland.Decision.Presence {

	// Base class for selecting from deployed presence
	public class Deployed : TypedDecision<Space> {
		public Deployed(string prompt, Spirit spirit):base(prompt, spirit.Presence.Spaces, Present.Always ) { }
	}

}
