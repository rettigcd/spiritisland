namespace SpiritIsland.BranchAndClaw;

public class ImmigrationSlows : IFearOptions {
	public const string Name = "Immigration Slows";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "During the next normal build, skip the lowest numbered land matching the invader card on each board." )]
	public Task Level1( GameCtx ctx ) {

		ctx.GameState.PreBuilding.ForRound.Add( ( args ) => {

			// During the next normal build, skip the lowest numbered land matching the invader card on each board

			// !! If this goes after something else has already removed the lowest # build, this will remove a higher # land.
			var spacesToSkip = args.SpacesWithBuildTokens
				.GroupBy( s => s.Space.Board )
				.SelectMany( grp => grp.OrderBy( x => x.Space.Label ).Take( 1 ) )
				.ToArray();

			foreach(var space in spacesToSkip)
				args.GameState.AdjustTempToken(space.Space, BuildStopper.Default( Name ) ); // !!! replace event handler with token that does the lowest #d land check

		} );

		return Task.CompletedTask;
	}

	[FearLevel( 2, "Skip the next normal build. The build card remains in place instead of shifting left." )]
	public Task Level2( GameCtx ctx ) {
		var build = ctx.GameState.InvaderDeck.Build;
		// Skip the next normal build.
		build.SkipNextNormal();
		// The build card remains in place instead of shifting left
		build.HoldNextBack();

		return Task.CompletedTask;
	}

	[FearLevel( 3, "Skip the next normal build.  The build card shifts left as usual." )]
	public Task Level3( GameCtx ctx ) {
		// Skip the next normal build.
		ctx.GameState.InvaderDeck.Build.SkipNextNormal();
		// The build card shifts left as usual
		return Task.CompletedTask;
	}

}