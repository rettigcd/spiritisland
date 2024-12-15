namespace SpiritIsland;

static public class SpiritAction_Extensions {
	static public GrowthAction ToGrowth( this SpiritAction cmd ) => new GrowthAction( cmd, Phase.Init );
	static public FastSlowAction ToFastSlow( this SpiritAction cmd ) => new FastSlowAction(cmd);
}
