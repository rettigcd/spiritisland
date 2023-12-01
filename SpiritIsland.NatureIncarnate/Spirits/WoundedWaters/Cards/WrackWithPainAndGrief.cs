namespace SpiritIsland.NatureIncarnate;

public class WrackWithPainAndGrief {

	[SpiritCard( "Wrack with Pain and Grief", 1, Element.Water, Element.Plant, Element.Animal ), Slow]
	[FromPresence( Filter.Blight, 1 )]
	[Instructions( "2 Fear. Push 1 Explorer and 1 Town." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		ctx.AddFear(2);
		await ctx.SourceSelector
			.AddGroup(1,Human.Explorer)
			.AddGroup(1,Human.Town)
			.PushN(ctx.Self);
	}

}
