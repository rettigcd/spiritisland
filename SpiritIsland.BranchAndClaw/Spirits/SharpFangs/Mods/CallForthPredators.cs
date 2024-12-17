namespace SpiritIsland.BranchAndClaw;

public class CallForthPredators() 
	: SpiritAction( Name, MyActAsync )
{

	public const string Name = "Call Forth Predators";
	const string Description1 = "During each Spirit Phase, you may replace 1 of your Presence with 1 Beasts. The replaced Presence leaves the game.";
	static public SpecialRule Rule => new SpecialRule(Name, Description1);

	static async Task MyActAsync(Spirit spirit) {
		var token = await spirit.SelectAsync( new A.SpaceTokenDecision("Replace 1 Presence with 1 Beast", spirit.Presence.Deployed, Present.Done ) );
		if( token is null ) return;
		await token.Space.ReplaceAsync(token.Token,1, Token.Beast);
	}

}
