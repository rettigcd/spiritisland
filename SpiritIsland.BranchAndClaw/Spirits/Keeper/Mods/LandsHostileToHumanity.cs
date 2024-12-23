
namespace SpiritIsland.BranchAndClaw;

public class LandsHostileToHumanity(Spirit spirit) : SpiritPresenceToken(spirit), IConfigRavages, IHandleTokenAdded {

	public const string Name = "Lands Hostile to Humanity";
	const string Description = "Your Sacred Sites may also count as Badlands.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	public Task HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
		if( args.Added == this && args.To is Space space )
			UpdateBadlands(space);
		return Task.CompletedTask;
	}

	public override Task HandleTokenRemovedAsync(ITokenRemovedArgs args) {
		if( args.Removed == this && args.From is Space space)
			UpdateBadlands(space);
		return base.HandleTokenRemovedAsync(args);
	}

	public async Task Config(Space space) {
		if( !ShouldHaveBadlands(space)
			|| !await Self.UserSelectsFirstText($"Use Sacred Site as Badlands for Ravage on {space.Label}?", "Yes, stick it to them!", "No, we'll let them off the hook.")
		) {
			// Temporarily remove badlands.
			space.Init(Badlands, 0);
			ActionScope.Current.AtEndOfThisAction(scope => UpdateBadlands(space)); // re-evaluate at end of action.
		}
	}

	void UpdateBadlands(Space space) {
		space.Init(Badlands, ShouldHaveBadlands(space) ? 1 : 0);
	}

	bool ShouldHaveBadlands(Space space) => Self.Presence.IsSacredSite(space);

	TokenVariety Badlands = new TokenVariety(Token.Badlands, "😀");

}
