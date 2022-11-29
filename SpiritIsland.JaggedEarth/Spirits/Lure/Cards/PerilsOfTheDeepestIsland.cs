namespace SpiritIsland.JaggedEarth;

public class PerilsOfTheDeepestIsland {

	[SpiritCard("Perils of the Deepest Island",1,Element.Moon,Element.Plant,Element.Animal),Slow,FromPresence(0,Target.Inland)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		// 1 fear.
		ctx.AddFear(1);

		// add 1 badlands.
		await ctx.Badlands.Add(1);

		// Add 1 beast within 1 range.
		var options = ctx.Range(1, TargetingPowerType.PowerCard );

		var spaceCtx = await ctx.SelectSpace("Add beast", options.Select(x=>x.Space));

		await spaceCtx.Beasts.Add(1);

		// Push up to 2 dahan.
		await ctx.PushUpToNDahan(2);
	}

}