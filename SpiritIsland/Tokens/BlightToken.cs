namespace SpiritIsland;

public class BlightToken( string label, char k, Img img ) 
	: TokenClassToken( label, k, img )
	, IHandleTokenAddedAsync
	, IHandleTokenRemoved
{
	public async Task HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {

		if(args.Added != this) return; // token-added event handler for blight only
		if(!ShouldDoBlightAddedEffects( args.Reason )) return;

		var gs = GameState.Current;
		BlightConfig config = Config.Value;

		config.BlightFromCardTrigger = args;

		// remove from source (usually card)
		await gs.TakeBlightFromCard( args.Count );

		// Destory presence
		if(config.DestroyPresence)
			foreach(Spirit spirit in gs.Spirits)
				// I would like to replace this with:
				if( spirit.Presence.IsOn( to ) )
					await to.Destroy( spirit.Presence.TokensDeployedOn(to).First(), 1 ); // !!! Not correct for Incarna

		// Cascade blight
		if( to.Blight.Count != 1 && config.ShouldCascade ) {
			Space cascadeTo = await gs.Spirits[0].SelectAsync( A.Space.ForMoving_SpaceToken(
				$"Cascade blight from {to.Space.Label} to",
				to.Space,
				gs.CascadingBlightOptions( to ).Downgrade(),
				Present.Always,
				Token.Blight
			) );
			await cascadeTo.Tokens.Blight.AddAsync( 1, args.Reason ); // Cascading blight shares original blights reason.
		}

	}

	static bool ShouldDoBlightAddedEffects( AddReason reason ) {
		return reason switch {
			AddReason.AsReplacement => false,
			AddReason.MovedTo => false,
			AddReason.Added => true, // Generic add
			AddReason.Ravage => true, // blight from ravage
			AddReason.BlightedIsland => true, // blight from blighted island card
			AddReason.SpecialRule => true, // Heart of wildfire - Blight from add presence
			_ => throw new ArgumentOutOfRangeException( nameof( reason ) )
		};
	}

	public void HandleTokenRemoved( SpaceState from, ITokenRemovedArgs args ) {
		if(args.Removed == Token.Blight
			&& !args.Reason.IsOneOf(
				RemoveReason.MovedFrom, // pushing / gathering blight
				RemoveReason.TakingFromCard
				// RemoveReason.Replaced,  // acording to querki Replaced blight goes back to the card.
			)
		)	BlightCard.Space.Tokens.Adjust(this,1); // shouldn't be any modifying-add mods on blight card
	}

	static readonly ActionScopeValue<BlightConfig> Config
		= new("BlightConfig",()=>new BlightConfig());

	static public BlightConfig ForThisAction => Config.Value;
}

public class BlightConfig {

	public ITokenAddedArgs BlightFromCardTrigger;

	public bool ShouldCascade = true;
	public bool TakeFromCard = true;
	public bool DestroyPresence = true;
}