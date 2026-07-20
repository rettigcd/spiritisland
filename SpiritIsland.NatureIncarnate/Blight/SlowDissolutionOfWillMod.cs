namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Split from SlowDissolutionOfWill (the blight card): the recurring Invader-Phase behavior, and the
/// per-spirit token choice it depends on, live here instead - not entangled with the BlightCard's own
/// identity. Not ISpaceEntity, per the standing correction that IRunBeforeInvaderPhase-only types are
/// never placed on a Space (see docs/ISpaceEntity-Types.md). Registered with
/// PreInvaderPhaseActionRegistry, same tag-dispatch shape as BlightCardRegistry/SpaceEntitySerialization -
/// nothing calls PreInvaderPhaseActionRegistry.Deserialize end-to-end yet (that needs GameState's
/// hook-action-list serialization, docs/GameSerialization-Roadmap.md section 10), but this is ready
/// for whenever it does.
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

	public JsonArray ToJson( ISerializationContext ctx )
		=> new JsonArray( Tag, new JsonArray( _replacements.Select( p => (JsonNode)new JsonArray( ctx.IndexOf( p.Key ), ( (ITokenClass)p.Value ).Label ) ).ToArray() ) );

	const string Tag = "SlowDissolutionOfWillMod";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> PreInvaderPhaseActionRegistry.Register( Tag, ( json, ctx ) => {
			var mod = new SlowDissolutionOfWillMod();
			foreach( JsonNode? node in (JsonArray)json[1]! ) {
				var pair = (JsonArray)node!;
				mod._replacements[ ctx.SpiritAt( (int)pair[0]! ) ] = (IToken)ctx.TokenClassByLabel( pair[1]!.GetValue<string>() );
			}
			return mod;
		} );

}
