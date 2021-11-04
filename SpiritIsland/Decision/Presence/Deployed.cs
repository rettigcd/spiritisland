
using System.Collections.Generic;

namespace SpiritIsland.Decision.Presence {

	// Base class for selecting from deployed presence
	public class Deployed : TypedDecision<Space> {

		public static Deployed SourceForPlacing( Spirit spirit ) => new Deployed("Select Presence to place.", spirit);

		/// <summary> Target ALL spaces containing deployed presence </summary>
		public Deployed( string prompt, Spirit spirit )
			:base(prompt, spirit.Presence.Spaces, Present.Always ) 
		{ }

		/// <summary> Target SPECIFIC spaces containing deployed presence </summary>
		public Deployed( string prompt, IEnumerable<Space> fromSpaces, Present present )
			:base( prompt, fromSpaces, present )
		{}

	}


}
