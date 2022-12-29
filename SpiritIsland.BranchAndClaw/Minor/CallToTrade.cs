namespace SpiritIsland.BranchAndClaw;

public class CallToTrade {

	public const string Name = "Call to Trade";

	[MinorCard( Name, 1, Element.Air, Element.Water, Element.Earth, Element.Plant )]
	[Fast]
	[FromPresence( 1, Target.Dahan )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// You may Gather 1 dahan
		await ctx.GatherUpToNDahan(1);

		// If the Terror Level is 2 or lower
		if( ctx.GameState.Fear.TerrorLevel <= 2 ) {
			// Gather 1 town
			await ctx.Gather( 1, Invader.Town );

			// And the first ravage in target land becomes a build there instead.
			FirstRavageBecomesABuild( ctx );
		}

	}

	static void FirstRavageBecomesABuild( TargetSpaceCtx ctx ) {
		ctx.GameState.AdjustTempToken( ctx.Space, new ReplaceRavageWithBuild() );
	}

	class ReplaceRavageWithBuild : ActionModBaseToken, ISkipRavages {

		public ReplaceRavageWithBuild() : base( Name ) { }

		public Task<bool> Skip( GameState gameState, SpaceState space ) {
			space.Adjust( this, -1 );

			// Add Build
			space.Adjust( TokenType.DoBuild, 1 );
			// Stop Ravage
			return Task.FromResult(true);
		}
	}

}