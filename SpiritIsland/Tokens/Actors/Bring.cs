namespace SpiritIsland;

/// <summary>
/// Configures TokenMover to bring tokens allong
/// </summary>
static public class Bring {
	static public void FromAnywhere( TokenMover mover, Spirit spirit, Quota quota ) {
		mover
			.Track( async moved => {
				await new TokenMover( spirit, "Bring", moved.From, moved.To )
					.UseQuota( quota )
					.DoUpToN();
			} );
	}
}
