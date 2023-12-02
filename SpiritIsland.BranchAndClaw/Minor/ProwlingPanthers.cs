namespace SpiritIsland.BranchAndClaw;

public class ProwlingPanthers {

	[MinorCard( "Prowling Panthers", 1, Element.Moon, Element.Fire, Element.Animal ), Slow, FromPresence( 1, Filter.Jungle, Filter.Mountain )]
	[Instructions( "1 Fear. Add 1 Beasts. -or- If target land has Beasts, destroy 1 Explorer / Town." ), Artist( Artists.MoroRogers )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceAction( "1 fear, add beast", FearAndBeast ),
			new SpaceAction( "destroy 1 explorer/town", DestroyExplorerTown ).OnlyExecuteIf( x => x.Beasts.Any )
		);
	}

	static async Task FearAndBeast( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );
		await ctx.Beasts.AddAsync(1);
	}

	static Task DestroyExplorerTown( TargetSpaceCtx ctx ) {
		return ctx.Invaders.DestroyNOfAnyClass( 1, Human.Explorer_Town );
	}

}