﻿namespace SpiritIsland.BranchAndClaw;

public class ImmigrationSlows : IFearOptions {
	public const string Name = "Immigration Slows";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "During the next normal build, skip the lowest numbered land matching the invader card on each board." )]
	public Task Level1( FearCtx ctx ) {

		ctx.GameState.PreBuilding.ForRound.Add( ( args ) => {

			// During the next normal build, skip the lowest numbered land matching the invader card on each board

			// !! If this goes after something else has already removed the lowest # build, this will remove a higher # land.
			var spacesToSkip = args.SpaceCounts.Keys
				.GroupBy( s => s.Board )
				.SelectMany( grp => grp.OrderBy( x => x.Label ).Take( 1 ) )
				.ToArray();

			foreach(var space in spacesToSkip)
				args.Skip1(space);

		} );

		return Task.CompletedTask;
	}

	[FearLevel( 2, "Skip the next normal build. The build card remains in place instead of shifting left." )]
	public Task Level2( FearCtx ctx ) {
		var build = ctx.GameState.InvaderDeck.Build;
		// Skip the next normal build.
		build.SkipNextNormal();
		// The build card remains in place instead of shifting left
		build.HoldNextBack();

		return Task.CompletedTask;
	}

	[FearLevel( 3, "Skip the next normal build.  The build card shifts left as usual." )]
	public Task Level3( FearCtx ctx ) {
		// Skip the next normal build.
		ctx.GameState.InvaderDeck.Build.SkipNextNormal();
		// The build card shifts left as usual
		return Task.CompletedTask;
	}

}