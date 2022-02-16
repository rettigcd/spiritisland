namespace SpiritIsland.JaggedEarth;

[InnatePower("Slip the Flow of Time", "You may use this Power any number of times. Cost to Use: 1 Time per previous use this turn"), Fast, AnySpirit]
[RepeatWithTime]
class SlipTheFlowOfTime {

	[InnateOption("3 moon,1 air","Target Spirit may Resolve 1 slow Power now.")]
	static public Task Option1( TargetSpiritCtx ctx ) {
		return ResolveOutOfPhaseAction.Execute( ctx.OtherCtx );
	}

	[InnateOption("2 sun,2 moon","Target Spirit may Reclaim 1 Power Card from their discarded or played cards.",1)]
	static public Task Option2( TargetSpiritCtx ctx ) {
		return ctx.Other.Reclaim1FromDiscardOrPlayed();
	}

	[InnateOption("3 sun,2 air","Target Spirit may play a Power Card by paying its cost.",2)]
	static public Task Option3( TargetSpiritCtx ctx ) {
		return ctx.Other.SelectAndPlayCardsFromHand( 1 );
	}

}