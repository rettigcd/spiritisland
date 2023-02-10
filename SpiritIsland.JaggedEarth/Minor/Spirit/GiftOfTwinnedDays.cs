namespace SpiritIsland.JaggedEarth;

public class GiftOfTwinnedDays{

	public const string Name = "Gift of Twinned Days";

	[MinorCard(Name,1,Element.Sun,Element.Moon),Fast,AnotherSpirit]
	static public Task ActAsync( TargetSpiritCtx ctx ){
		// Once this turn, target spirit may repeat the lowest-cost Power Card they have in play by paying its cost again.
		ctx.Other.AddActionFactory(new RepeatCheapestCardForCost( Name ) );

		// You may do likewise.
		ctx.Self.AddActionFactory(new RepeatCheapestCardForCost( Name ) ); // Exclude: Gift of Twinned Days

		return Task.CompletedTask;
	}

}