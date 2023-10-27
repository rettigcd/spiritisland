namespace SpiritIsland.JaggedEarth;

public class TheShoreSeethesWithHatred{ 

	[MinorCard("The Shore Seethes With Hatred",1,Element.Fire,Element.Water,Element.Earth,Element.Plant),Slow,FromPresence(1,Target.Coastal)]
	[Instructions( "1  Fear. Add 1 Badlands and 1 Wilds." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// 1 fear
		ctx.AddFear(1);

		// add 1 badlands and 1 wilds
		await ctx.Badlands.AddAsync(1);
		await ctx.Wilds.AddAsync(1);
	}

}
