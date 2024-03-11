namespace SpiritIsland;

/// <summary>
/// Configures TokenMover to bring tokens allong
/// </summary>
static public class Bring {
	static public Func<TokenMovedArgs,Task> FromAnywhere( Spirit spirit, Quota quota ) {
		return async moved => {
			Space from = (Space)moved.From;
			Space to = (Space)moved.To;
			await new TokenMover( spirit, "Bring", from.ScopeTokens, to.ScopeTokens )
				.UseQuota( quota )
				.DoUpToN();
		};
	}

}
