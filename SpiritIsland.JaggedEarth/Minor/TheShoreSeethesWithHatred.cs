namespace SpiritIsland.JaggedEarth;

public class TheShoreSeethesWithHatred{ 

	[MinorCard("The Shore Seethes With Hatred",1,Element.Fire,Element.Water,Element.Earth,Element.Plant),Slow,FromPresence(1,Target.Coastal)]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// 1 fear
		ctx.AddFear(1);

		// add 1 badlands and 1 wilds
		await ctx.Badlands.Add(1);
		await ctx.Wilds.Add(1);
	}

}
