
namespace SpiritIsland {

	public class SelectInvaderToGatherDecision : InvadersOnSpaceDecision {
		public Space From { get; }
		public Space To { get; }
		public SelectInvaderToGatherDecision( Space from, Space to, InvaderSpecific[] options, Present present )
			: base( $"Invader to gather ({from.Label}=>{to.Label})", from, options, present ) {
			From = from;
			To = to;
		}
	}


}