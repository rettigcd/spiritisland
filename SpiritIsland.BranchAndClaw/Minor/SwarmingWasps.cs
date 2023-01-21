namespace SpiritIsland.BranchAndClaw;

public class SwarmingWasps {

	[MinorCard( "Swarming Wasps", 0, Element.Fire, Element.Air, Element.Animal )]
	[Fast]
	[FromPresence( 1, Target.NoBlight )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "Add 1 beast", ctx => ctx.Beasts.Add(1) ),
			new SpaceAction( "Push up to 2 explorers", ctx => ctx.PushUpTo( 2, Invader.Explorer ) )
				.OnlyExecuteIf( x=>x.Beasts.Any )
		);

	}

}