namespace SpiritIsland.JaggedEarth;

public class DrawMinorOnceAndPlayExtraCardThisTurn : GrowthActionFactory, IActionFactory {

	bool drewMinor = false;

	public override async Task ActivateAsync( SelfCtx ctx ) {

		if(!drewMinor)
			await ctx.DrawMinor();
		drewMinor = true;

		ctx.Self.tempCardPlayBoost++;
	}

}