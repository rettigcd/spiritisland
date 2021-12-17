using System.Threading.Tasks;

namespace SpiritIsland {
	/// <summary>
	/// Overrides Selecting destination with a fixed destination
	/// </summary>
	public class TokenPusher_FixedDestination : TokenPusher {
		readonly Space destination;
		public TokenPusher_FixedDestination( TargetSpaceCtx ctx, Space destination ) : base( ctx ) { 
			this.destination = destination;
		}

		protected override Task<Space> SelectDestination( Token token ) {
			return Task.FromResult(destination);
		}

	}

}