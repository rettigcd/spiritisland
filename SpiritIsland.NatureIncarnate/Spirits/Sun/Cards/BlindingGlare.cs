namespace SpiritIsland.NatureIncarnate;

public class BlindingGlare {
	const string Name = "Blinding Glare";

	[SpiritCard(Name,0,Element.Sun,Element.Air),Fast,FromSacredSite(0,Target.Invaders)]
	[Instructions( "2 Fear. -or- Skip up to one Ravage Action. -If you have- 5 Sun: Instead, 3 Fear. -or- Skip up to one Invader Action." ), Artist( Artists.AgnieszkaDabrowiecka )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		BaseCmd<TargetSpaceCtx> action = (await ctx.YouHave( "5 sun" ))
			? Cmd.Pick1( Cmd.AddFear( 3 ), Cmd.Skip1InvaderAction( Name ) ) // meet threshold
			: Cmd.Pick1( Cmd.AddFear( 2 ), Cmd.Skip1Ravage( Name ) ); // don't meet
		await action.ActAsync(ctx);
	}


}