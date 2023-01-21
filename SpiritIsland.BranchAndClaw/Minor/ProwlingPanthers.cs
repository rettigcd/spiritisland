namespace SpiritIsland.BranchAndClaw;

public class ProwlingPanthers {

	[MinorCard( "Prowling Panthers", 1, Element.Moon, Element.Fire, Element.Animal ), Slow, FromPresence( 1, Target.Jungle, Target.Mountain )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceAction( "1 fear, add beast", FearAndBeast ),
			new SpaceAction( "destroy 1 explorer/town", DestroyExplorerTown ).OnlyExecuteIf( x => x.Beasts.Any )
		);
	}

	static async Task FearAndBeast( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );
		await ctx.Beasts.Add(1);
	}

	static Task DestroyExplorerTown( TargetSpaceCtx ctx ) {
		return ctx.Invaders.DestroyNOfAnyClass( 1, Invader.Explorer_Town );
	}

}