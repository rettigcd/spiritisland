namespace SpiritIsland.BranchAndClaw;

public class ImmigrationSlows : IFearCard {
	public const string Name = "Immigration Slows";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }

	[FearLevel( 1, "During the next normal build, skip the lowest-numbered land matching the invader card on each board." )]
	public Task Level1( GameCtx ctx ) {

		// this is the next normal build,
		var card = ctx.GameState.InvaderDeck.Build.Cards.FirstOrDefault(); 
		if(card == null) return Task.CompletedTask; // !! should this fall over to the next round?

		foreach(var board in ctx.GameState.Island.Boards) {
			var lowest = ctx.GameState.Tokens.PowerUp(board.Spaces).FirstOrDefault(card.MatchesCard);
			if(lowest != null)
				lowest.Skip1Build( Name );
		}
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