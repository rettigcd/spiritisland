namespace SpiritIsland.JaggedEarth;

public class SuckingOoze{ 

	[MinorCard("Sucking Ooze",0,Element.Moon,Element.Water,Element.Earth),Fast,FromPresence(1, Filter.Sands, Filter.Wetland )]
	[Instructions( "2 Fear if Invaders are present. Isolate target land." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// 2 fear if Invaders are present.
		if(ctx.HasInvaders)
			await ctx.AddFear(2);

		// Isolate target land.
		ctx.Isolate();
	}

}