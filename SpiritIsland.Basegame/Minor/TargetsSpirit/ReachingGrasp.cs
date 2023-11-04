namespace SpiritIsland.Basegame;

public class ReachingGrasp {

	[MinorCard( "Reaching Grasp", 0, Element.Sun, Element.Air, Element.Water ),Fast,AnySpirit]
	[Instructions( "Target Spirit gets +2 Range with all their Powers." ), Artist( Artists.NolanNasser )]
	static public Task Act( TargetSpiritCtx ctx ) {

		// target spirit gets +2 range with all their Powers
		RangeCalcRestorer.Save(ctx.Other);
		RangeExtender.Extend( ctx.Other, 2 );

		return Task.CompletedTask;
	}

}