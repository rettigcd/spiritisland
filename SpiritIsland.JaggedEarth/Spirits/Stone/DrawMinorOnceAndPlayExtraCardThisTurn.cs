namespace SpiritIsland.JaggedEarth;

public class DrawMinorOnceAndPlayExtraCardThisTurn : GrowthActionFactory, ITrackActionFactory {

	bool drewMinor = false;

	public RunTime RunTime => RunTime.Before;// no growth dependencies

	public override async Task ActivateAsync( SelfCtx ctx ) {

		if(!drewMinor)
			await ctx.DrawMinor();
		drewMinor = true;

		ctx.Self.tempCardPlayBoost++;
	}

}