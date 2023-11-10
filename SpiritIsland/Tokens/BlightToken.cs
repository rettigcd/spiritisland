namespace SpiritIsland;

public class BlightToken : TokenClassToken
	, IHandleTokenAddedAsync
	, IHandleTokenRemoved
{
	public BlightToken( string label, char k, Img img ) : base( label, k, img, TokenCategory.Blight ) {}

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {

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
				if( spirit.Presence.IsOn( args.To ) )
					await args.To.Destroy( spirit.Presence.TokensDeployedOn(args.To.Space).First(), 1 ); // !!! Not correct for Incarna

		// Cascade blight
		if( args.To.Blight.Count != 1 && config.ShouldCascade ) {
			Space cascadeTo = await gs.Spirits[0].Select( A.Space.ForMoving_SpaceToken(
				$"Cascade blight from {args.To.Space.Label} to",
				args.To.Space,
				gs.CascadingBlightOptions( args.To ).Downgrade(),
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

	public void HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(args.Removed == Token.Blight
			&& !args.Reason.IsOneOf(
				RemoveReason.MovedFrom, // pushing / gathering blight
				RemoveReason.Replaced,  // just in case...
				RemoveReason.TakingFromCard
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