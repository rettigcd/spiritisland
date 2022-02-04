namespace SpiritIsland.JaggedEarth;
public class JaggedShardsPushFromTheEarth {

	[SpiritCard("Jagged Shards Push from the Earth",0,Element.Fire,Element.Earth), Slow, FromPresence(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// Add 1 badland.
		await ctx.Badlands.Add(1);
		// Push up to 2 dahan.
		await ctx.PushUpToNDahan(2);
	}

}