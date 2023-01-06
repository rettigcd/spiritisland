namespace SpiritIsland.BranchAndClaw;

public class AnimatedWrackroot {

	[MinorCard( "Animated WrackRoot", 0, Element.Moon, Element.Fire, Element.Plant )]
	[Slow]
	[FromPresence( 0 )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceAction( "1 fear, Destroy 1 explorer", FearAndExplorer ),
			new SpaceAction("add 1 wilds", ctx => ctx.Wilds.Add(1) )
		);
	}

	private static async Task FearAndExplorer( TargetSpaceCtx ctx ) {
		// 1 fear
		ctx.AddFear( 1 );
		// destroy 1 explorer
		await ctx.Invaders.DestroyNOfClass( 1, Invader.Explorer );
	}

}