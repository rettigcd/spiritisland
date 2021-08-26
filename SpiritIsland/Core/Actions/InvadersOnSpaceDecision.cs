using System.Threading.Tasks;

namespace SpiritIsland {

	public class InvadersOnSpaceDecision : SelectAsync<InvaderSpecific> {
		public InvadersOnSpaceDecision( string prompt, Space space, InvaderSpecific[] options, Present present, TaskCompletionSource<InvaderSpecific> promise )
			: base( prompt, options, present, promise ) { 
			Space = space;
		}
		public Space Space { get; }
	}

}