
namespace SpiritIsland.Decision {

	public class TokenToPush : TokenOnSpace {

		public TokenToPush( Space space, int count, Token[] options, Present present )
			: base( present != Present.Done ? $"Push ({count})" : $"Push up to ({count})", space, options, present ) 
		{ }

	}
}