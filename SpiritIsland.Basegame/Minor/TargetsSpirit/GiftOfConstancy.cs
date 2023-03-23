namespace SpiritIsland.Basegame;

public class GiftOfConstancy {

	[MinorCard( "Gift of Constancy", 0, Element.Sun, Element.Earth ),Fast,AnySpirit]
	[Instructions( "Target Spirit gains 2 Energy. At end of turn, target Spirit may Reclaim 1 Power Card instead of discarding it. If you target another Spirit, you may also Reclaim 1 Power Card instead of discarding it." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync( TargetSpiritCtx ctx ) {

		// target spirit gains 2 energy.  
		ctx.Other.Energy += 2;

		// At end of turn, target spirit may reclaim 1 power card instead of discarding it.
		var purchased = ctx.Other.InPlay;
		ctx.GameState.TimePasses_ThisRound.Push( new Reclaim1InsteadOfDiscard( ctx.Other ).Reclaim );

		// if you target another spirit you may also reclaim 1 power Card instead of discarding it.
		if(ctx.Other != ctx.Self)
			ctx.GameState.TimePasses_ThisRound.Push( new Reclaim1InsteadOfDiscard( ctx.Self ).Reclaim );

		return Task.CompletedTask;
	}

}