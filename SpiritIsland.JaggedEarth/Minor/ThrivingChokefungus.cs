namespace SpiritIsland.JaggedEarth;

public class ThrivingChokefungus{ 

	[MinorCard("Thriving Chokefungus",1,Element.Moon,Element.Water,Element.Plant),Slow,FromSacredSite(1, Target.Jungle,Target.Wetland)]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// add 1 disease
		await ctx.Disease.Add(1);

		// add 1 badlands
		await ctx.Badlands.Add(1);
	}

}