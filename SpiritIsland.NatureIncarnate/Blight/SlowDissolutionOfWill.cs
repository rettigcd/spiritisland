namespace SpiritIsland.NatureIncarnate;

public class SlowDissolutionOfWill : BlightCard {

	public SlowDissolutionOfWill()
		:base("Slow Dissolution of Will", 
			"Immediately: Each Spirit chooses one of Badlands, Beast, or Wilds. Each Invader Phase: Each Spirit Replaces 1 Presence with their chosen type of Spirit Token.", 
			3
		) 
	{}

	public override IActOn<GameState> Immediately => Cmd.Multiple( 
		new BaseCmd<Spirit>( ChooseTokenPrompt, ChooseToken ).ForEachSpirit(), 
		new BaseCmd<Spirit>( ReplacePrompt, DoReplace ).ForEachSpirit().AtTheStartOfEachInvaderPhase()
	);

	async Task ChooseToken(Spirit spirit) 
		=> _replacements[spirit] = await spirit.SelectAlways( ChooseTokenPrompt, [Token.Badlands, Token.Beast, Token.Wilds] );


	async Task DoReplace(Spirit spirit ) {
		var replacement = _replacements[spirit];
		SpaceToken spaceToken = await spirit.SelectAlways("Replace Presence with " + replacement.Text, spirit.Presence.Deployed);

		await spaceToken.Destroy(); // .Destroy(spirit.Presence.Token,1);
		await spaceToken.Space.AddAsync(replacement,1);
	}

	readonly Dictionary<Spirit,IToken> _replacements = [];

	const string ChooseTokenPrompt = "Choose Badlands, Beast, or Wilds as Spirit-Replacement token";
	const string ReplacePrompt = "Replaces 1 Presence with their chosen type of Spirit Token.";
}
