
namespace SpiritIsland {

	public class SelectTokenToGatherDecision : InvadersOnSpaceDecision {
		public Space From { get; }
		public Space To { get; }
		public SelectTokenToGatherDecision( Space from, Space to, Token[] options, Present present )
			: base( $"Invader to gather ({from.Label}=>{to.Label})", from, options, present ) {
			From = from;
			To = to;
		}
	}


}