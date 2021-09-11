
namespace SpiritIsland.Decision {

	public class TokenToPush : TokenOnSpace {

		public TokenToPush( Space space, int count, Token[] options, Present present )
			: base( $"Select item to push ({count} remaining)", space, options, present ) { }
	}


}