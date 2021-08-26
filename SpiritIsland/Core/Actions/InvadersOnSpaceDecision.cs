using System.Threading.Tasks;

namespace SpiritIsland {

	public class InvadersOnSpaceDecision : TypedDecision<InvaderSpecific> {
		public InvadersOnSpaceDecision( string prompt, Space space, InvaderSpecific[] options, Present present )
			: base( prompt, options, present ) { 
			Space = space;
		}
		public Space Space { get; }
	}

}