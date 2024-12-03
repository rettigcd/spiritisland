namespace SpiritIsland;

public class BlightToken( string label, char k, Img img ) 
	: TokenClassToken( label, k, img )
	, IHandleTokenAdded
	, IHandleTokenRemoved
{

	/// <summary>
	/// Triggers: Destroy presence, cascade, remove from Blight card
	/// </summary>
	/// <remarks>
	/// Called when added to regular space AND when added to card.
	/// </remarks>
	public async Task HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
		if (args.Added != this) return; // token-added event handler for blight only
		if( !ShouldDoBlightAddedEffects(args.Reason) ) return;

		var gs = GameState.Current;
		BlightConfig config = Config.Value;

		config.BlightFromCardTrigger = args;

		// remove from source (usually card)
		await gs.BlightCard.TakeBlight(args.Count);

		// Destory presence
		if( config.DestroyPresence )
			await DestroyPresence(to, gs);

		// Cascade blight
		if (to.Blight.Count != 1 && config.ShouldCascade) {

			var terrain = gs.Terrain_ForBlight;
			var cascadeOptions = to.Adjacent_Existing.Where(x => !terrain.MatchesTerrain(x, Terrain.Ocean) // normal case,
			 || terrain.MatchesTerrain(x, Terrain.Wetland));

			Space cascadeTo = await gs.Spirits[0].SelectAsync(A.SpaceDecision.ForMoving(
				$"Cascade blight from {to.SpaceSpec.Label} to",
				to.SpaceSpec,
				cascadeOptions,
				Present.Always,
				Token.Blight
			));
			await cascadeTo.Blight.AddAsync(1, args.Reason); // Cascading blight shares original blights reason.
		}

	}

	/// <summary>
	/// Triggers returning blight to card
	/// </summary>
	public async Task HandleTokenRemovedAsync( Space from, ITokenRemovedArgs args ) {
		bool shouldReturnBlightToCard = args.Removed == Token.Blight
			&& !args.Reason.IsOneOf(
				RemoveReason.MovedFrom, // pushing / gathering blight
				RemoveReason.TakingFromCard
				// RemoveReason.Replaced,  // acording to querki Replaced blight goes back to the card.
			);
		if( shouldReturnBlightToCard )
			await ActionScope.Current.GameState.BlightCard.ReturnBlight(1);
	}

	public static BlightConfig ScopeConfig => Config.Value;

	#region private helpers methods

	static async Task DestroyPresence(Space to, GameState gs) {
		foreach (Spirit spirit in gs.Spirits)
			// I would like to replace this with:
			if (spirit.Presence.IsOn(to))
				await to.Destroy(spirit.Presence.TokensDeployedOn(to).First(), 1); // !!! Not correct for Incarna
	}

	static bool ShouldDoBlightAddedEffects(AddReason reason) {
		return reason switch {
			AddReason.AsReplacement  => false,
			AddReason.MovedTo        => false,
			AddReason.AddedToCard    => false,	// returning blight to blight card
			AddReason.Added          => true,	// Generic add
			AddReason.Ravage         => true,	// blight from ravage
			AddReason.BlightedIsland => true,	// blight from blighted island card
			AddReason.SpecialRule    => true,	// Heart of wildfire - Blight from add presence
			_                        => throw new ArgumentOutOfRangeException(nameof(reason))
		};
	}
	#endregion private helper methods

	static readonly ActionScopeValue<BlightConfig> Config = new("BlightConfig", () => new BlightConfig());

}

public class BlightConfig {

	public ITokenAddedArgs BlightFromCardTrigger;

	public bool ShouldCascade = true;
	public bool TakeFromCard = true;
	public bool DestroyPresence = true;
}