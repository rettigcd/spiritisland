namespace SpiritIsland.JaggedEarth;

public class SuckingOoze{ 

	[MinorCard("Sucking Ooze",0,Element.Moon,Element.Water,Element.Earth),Fast,FromPresence(1,Target.SandOrWetland)]
	static public Task ActAsync(TargetSpaceCtx ctx){
		// 2 fear if Invaders are present.
		if(ctx.HasInvaders)
			ctx.AddFear(2);

		// Isolate target land.
		ctx.Isolate();

		return Task.CompletedTask;
	}

}