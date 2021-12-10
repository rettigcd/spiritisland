
using System.Collections.Generic;

namespace SpiritIsland.Decision.Presence {

	// Base class for selecting from deployed presence
	public class Deployed : TypedDecision<Space> {

		public static Deployed SourceForPushing( Spirit spirit, Present present=Present.Done ) => new Deployed("Select Presence to push.", spirit, present);


		/// <summary> Target ALL spaces containing deployed presence </summary>
		public Deployed( string prompt, Spirit spirit, Present present = Present.Always )
			:base(prompt, spirit.Presence.Spaces, present ) 
		{ }

		/// <summary> Target SPECIFIC spaces containing deployed presence </summary>
		public Deployed( string prompt, IEnumerable<Space> fromSpaces, Present present )
			:base( prompt, fromSpaces, present )
		{}

	}


}
