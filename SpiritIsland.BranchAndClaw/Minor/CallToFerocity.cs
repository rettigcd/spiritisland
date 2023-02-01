namespace SpiritIsland.BranchAndClaw;

public class CallToFerocity {

	[MinorCard( "Call to Ferocity", 0, Element.Sun, Element.Fire, Element.Earth )]
	[Slow]
	[FromPresence( 1, Target.Invaders )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceAction( "Gather up to 3 dahan", ctx => ctx.GatherUpToNDahan( 3 ) ),
			new SpaceAction( "1 fear and push 1 explorer and 1 town", FearAndPushExplorerAndTown ).OnlyExecuteIf( x=>x.Dahan.Any )
		);
	}

	static async Task FearAndPushExplorerAndTown( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );
		await ctx.Pusher
			.AddGroup( 1, Human.Explorer )
			.AddGroup( 1, Human.Town )
			.MoveN();
	}
}