namespace SpiritIsland {

	public class InvadersOnSpaceDecision : TypedDecision<InvaderSpecific> {
		protected InvadersOnSpaceDecision( string prompt, Space space, InvaderSpecific[] options, Present present )
			: base( prompt, options, present ) { 
			Space = space;
		}
		public Space Space { get; }
	}

}