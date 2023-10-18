namespace SpiritIsland.BranchAndClaw;

public class AnimatedWrackroot {

	public const string Name = "Animated Wrackroot";

	[MinorCard( Name, 0, Element.Moon, Element.Fire, Element.Plant ),Slow,FromPresence( 0 )]
	[Instructions( "1 Fear. Destroy 1 Explorer. -or- Add 1 Wilds." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceCmd( "1 fear, Destroy 1 explorer", FearAndExplorer ),
			new SpaceCmd("add 1 wilds", ctx => ctx.Wilds.Add(1) )
		);
	}

	private static async Task FearAndExplorer( TargetSpaceCtx ctx ) {
		// 1 fear
		ctx.AddFear( 1 );
		// destroy 1 explorer
		await ctx.Invaders.DestroyNOfClass( 1, Human.Explorer );
	}

}