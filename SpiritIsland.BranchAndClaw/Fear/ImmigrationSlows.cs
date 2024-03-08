namespace SpiritIsland.BranchAndClaw;

public class ImmigrationSlows : FearCardBase, IFearCard {
	public const string Name = "Immigration Slows";
	public string Text => Name;

	[FearLevel( 1, "During the next normal build, skip the lowest-numbered land matching the Invader card on each board." )]
	public Task Level1( GameState gs ) {

		// this is the next normal build,
		var card = gs.InvaderDeck.Build.Cards.FirstOrDefault(); 
		if(card == null) return Task.CompletedTask; // !! should this fall over to the next round?

		foreach(var board in gs.Island.Boards) {
			var lowest = board.Spaces.Tokens().FirstOrDefault(card.MatchesCard);
			lowest?.Skip1Build( Name );
		}
		return Task.CompletedTask;
	}

	[FearLevel( 2, "Skip the next normal Build. The Build card remains in place instead of shifting left." )]
	public Task Level2( GameState gs ) {
		var build = gs.InvaderDeck.Build;
		// Skip the next normal build.
		build.SkipNextNormal();
		// The build card remains in place instead of shifting left
		build.HoldNextBack();

		return Task.CompletedTask;
	}

	[FearLevel( 3, "Skip the next normal Build. The Build card shifts left as usual." )]
	public Task Level3( GameState ctx ) {
		// Skip the next normal build.
		ctx.InvaderDeck.Build.SkipNextNormal();
		// The build card shifts left as usual
		return Task.CompletedTask;
	}

}