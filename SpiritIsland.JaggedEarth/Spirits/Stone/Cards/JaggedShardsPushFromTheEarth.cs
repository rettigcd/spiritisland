namespace SpiritIsland.JaggedEarth;
public class JaggedShardsPushFromTheEarth {

	[SpiritCard("Jagged Shards Push From the Earth",0,Element.Fire,Element.Earth), Slow, FromPresence(1)]
	[Instructions( "Add 1 Badlands. Push up to 2 Dahan." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// Add 1 badland.
		await ctx.Badlands.Add(1);
		// Push up to 2 dahan.
		await ctx.PushUpToNDahan(2);
	}

}