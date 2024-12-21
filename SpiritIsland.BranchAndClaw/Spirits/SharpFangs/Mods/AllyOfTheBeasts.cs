namespace SpiritIsland.BranchAndClaw;

public class AllyOfTheBeasts(Spirit spirit) : FollowingPresenceToken(spirit, Token.Beast, "SFBtB") {
	public const string Name = "Ally of the Beasts";
	const string Description = "Your presensee may move with beast.";
	static public SpecialRule Rules => new SpecialRule(Name, Description);
}
