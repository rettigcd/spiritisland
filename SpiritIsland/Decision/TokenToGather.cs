
namespace SpiritIsland.Decision {

	public class TokenToGather : TokenOnSpace {

		public Space From { get; }
		public Space To { get; }
		public TokenToGather( Space from, Space to, Token[] options, Present present )
			: base( $"Invader to gather ({from.Label}=>{to.Label})", from, options, present ) {
			From = from;
			To = to;
		}

	}

}