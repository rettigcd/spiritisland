namespace SpiritIsland.BranchAndClaw;

public class CallToFerocity {

	[MinorCard( "Call to Ferocity", 0, Element.Sun, Element.Fire, Element.Earth ),Slow,FromPresence( 1, Filter.Invaders )]
	[Instructions( "Gather up to 3 Dahan. -or- If target land has Dahan, 1 Fear and Push 1 Explorer and 1 Town." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceAction( "Gather up to 3 dahan", ctx => ctx.GatherUpToNDahan( 3 ) ),
			new SpaceAction( "1 fear and push 1 explorer and 1 town", FearAndPushExplorerAndTown ).OnlyExecuteIf( x=>x.Dahan.Any )
		);
	}

	static async Task FearAndPushExplorerAndTown( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );
		await ctx.SourceSelector
			.AddGroup( 1, Human.Explorer )
			.AddGroup( 1, Human.Town )
			.PushN( ctx.Self );
	}
}