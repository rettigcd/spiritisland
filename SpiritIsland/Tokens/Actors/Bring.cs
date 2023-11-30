namespace SpiritIsland;

/// <summary>
/// Configures TokenMover to bring tokens allong
/// </summary>
static public class Bring {
	static public Func<TokenMovedArgs,Task> FromAnywhere( Spirit spirit, Quota quota ) {
		return async moved => {
			await new TokenMover( spirit, "Bring", moved.From, moved.To )
				.UseQuota( quota )
				.DoUpToN();
		};
	}

}
