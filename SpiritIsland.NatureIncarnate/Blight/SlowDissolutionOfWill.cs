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
		=> _replacements[spirit] = await spirit.Select<IToken>(ChooseTokenPrompt,new IToken[]{ Token.Badlands, Token.Beast, Token.Wilds },Present.Always);

	async Task DoReplace(Spirit spirit ) {
		var replacement = _replacements[spirit];
		Space space = await spirit.SelectDeployed("Replace Presence with " + replacement.Text );
		SpaceState tokens = space.Tokens;
		await tokens.Destroy(spirit.Presence.Token,1);
		await tokens.AddAsync(replacement,1);
	}

	readonly Dictionary<Spirit,IToken> _replacements = new();

	const string ChooseTokenPrompt = "Choose Badlands, Beast, or Wilds as Spirit-Replacement token";
	const string ReplacePrompt = "Replaces 1 Presence with their chosen type of Spirit Token.";
}
