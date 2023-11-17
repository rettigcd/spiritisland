namespace SpiritIsland.JaggedEarth;

public class SuckingOoze{ 

	[MinorCard("Sucking Ooze",0,Element.Moon,Element.Water,Element.Earth),Fast,FromPresence(1, Target.Sands, Target.Wetland )]
	[Instructions( "2 Fear if Invaders are present. Isolate target land." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync(TargetSpaceCtx ctx){
		// 2 fear if Invaders are present.
		if(ctx.HasInvaders)
			ctx.AddFear(2);

		// Isolate target land.
		ctx.Isolate();

		return Task.CompletedTask;
	}

}