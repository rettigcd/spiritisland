
using System.Collections.Generic;

namespace SpiritIsland.Decision.Presence {

	// Base class for selecting from deployed presence
	public class Deployed : TypedDecision<Space> {

		/// <summary> Target ALL spaces containing deployed presence </summary>
		public Deployed(string prompt, Spirit spirit)
			:base(prompt, spirit.Presence.Spaces, Present.Always ) 
		{ }

		/// <summary> Target SPECIFIC spaces containing deployed presence </summary>
		public Deployed( string prompt, IEnumerable<Space> spaces, Present present )
			:base( prompt, spaces, present )
		{}

	}

}
