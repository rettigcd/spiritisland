namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Split from SlowDissolutionOfWill (the blight card): the recurring Invader-Phase behavior, and the
/// per-spirit token choice it depends on, live here instead - not entangled with the BlightCard's own
/// identity.
/// </summary>
class SlowDissolutionOfWillMod : IRunBeforeInvaderPhase {

	public const string ChooseTokenPrompt = "Choose Badlands, Beast, or Wilds as Spirit-Replacement token";
	const string ReplacePrompt = "Replaces 1 Presence with their chosen type of Spirit Token.";

	bool IRunBeforeInvaderPhase.RemoveAfterRun => false;

	Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState )
		=> new BaseCmd<Spirit>( ReplacePrompt, DoReplace ).ForEachSpirit().ActAsync( gameState );

	public async Task ChooseToken( Spirit spirit )
		=> _replacements[spirit] = await spirit.SelectAlways( ChooseTokenPrompt, [Token.Badlands, Token.Beast, Token.Wilds] );

	async Task DoReplace( Spirit spirit ) {
		var replacement = _replacements[spirit];
		SpaceToken spaceToken = await spirit.SelectAlways("Replace Presence with " + replacement.Text, spirit.Presence.Deployed);

		await spaceToken.Destroy(); // .Destroy(spirit.Presence.Token,1);
		await spaceToken.Space.AddAsync(replacement,1);
	}

	readonly Dictionary<Spirit,IToken> _replacements = [];

}
