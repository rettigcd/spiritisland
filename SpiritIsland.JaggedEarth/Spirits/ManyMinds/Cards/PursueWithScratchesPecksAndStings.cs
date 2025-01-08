namespace SpiritIsland.JaggedEarth;

public class PursueWithScratchesPecksAndStings {

	[SpiritCard("Pursue With Scratches, Pecks, and Stings",0, Element.Sun, Element.Fire, Element.Air, Element.Animal), Fast, FromPresence(2, Filter.Beast)]
	[Instructions( "1 Fear. For each Beasts past the first, Push 1 Explorer / Town." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 1 fear
		await ctx.AddFear(1);
		// for each beast past the first, Push 1 explorer / town.
		int pushCount = ctx.Beasts.Count-1;
		if(0 < pushCount)
			await ctx.SourceSelector
				.UseQuota(new Quota().AddGroup(pushCount,Human.Explorer_Town))
				.PushUpToN(ctx.Self);
	}

}