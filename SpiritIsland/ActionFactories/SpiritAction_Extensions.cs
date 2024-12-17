namespace SpiritIsland;

static public class SpiritAction_Extensions {

	/// <summary>
	/// Convert SpiritAction to GrowthAction
	/// </summary>
	static public GrowthAction ToGrowth( this SpiritAction cmd ) 
		=> new GrowthAction( cmd, Phase.Init );

	/// <summary>
	/// Convert SpiritAction to Fast/SlowAction
	/// </summary>
	static public FastSlowAction ToFastSlow( this SpiritAction cmd ) 
		=> new FastSlowAction(cmd);
}
