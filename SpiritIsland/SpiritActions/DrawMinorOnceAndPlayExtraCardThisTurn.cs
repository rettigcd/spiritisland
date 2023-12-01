namespace SpiritIsland.JaggedEarth;

public class DrawMinorOnceAndPlayExtraCardThisTurn : SpiritAction {

	public DrawMinorOnceAndPlayExtraCardThisTurn():base( "DrawMinorOnceAndPlayExtraCardThisTurn" ) { }


	bool drewMinor = false;

	public override async Task ActAsync( SelfCtx ctx ) {

		if(!drewMinor)
			await ctx.DrawMinor();
		drewMinor = true;

		ctx.Self.tempCardPlayBoost++;
	}

}