namespace SpiritIsland.JaggedEarth;

public class ThrivingChokefungus{ 

	[MinorCard("Thriving Chokefungus",1,Element.Moon,Element.Water,Element.Plant),Slow,FromSacredSite(1, Filter.Jungle,Filter.Wetland)]
	[Instructions( "Add 1 Disease and 1 Badlands." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// add 1 disease
		await ctx.Disease.AddAsync(1);

		// add 1 badlands
		await ctx.Badlands.AddAsync(1);
	}

}