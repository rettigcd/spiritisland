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
				if(args.To.Has( spirit.Token ))
					await args.To.Destroy( spirit.Token, 1 );

		// Cascade blight
		if( args.To.Blight.Count != 1 && config.ShouldCascade ) {
			Space cascadeTo = await gs.Spirits[0].Gateway.Decision( Select.ASpace.ForMoving_SpaceToken(
				$"Cascade blight from {args.To.Space.Label} to",
				args.To.Space,
				gs.CascadingBlightOptions( args.To ),
				Present.Always,
				Token.Blight
			) );
			await cascadeTo.Tokens.Blight.Add( 1, args.Reason ); // Cascading blight shares original blights reason.
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


public class LandDamage : IToken, IHandleTokenAddedAsync {

	public static readonly LandDamage Token = new LandDamage();

	#region Extra Token stuff we don't use
	public Img Img => Img.None;
	public IEntityClass Class => ActionModTokenClass.Mod;
	public string Text => "Land Damage";
	#endregion

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
		if(args.Added != this) return;

		// Land Damage cleans itself up at end of Action
		ActionScope.Current.AtEndOfThisAction( _ => { args.To.Init(this,0); } );

		// Add Blight
		if(GameState.Current.DamageToBlightLand <= args.To[this])
			await args.To.Blight.Add( 1, AddReason.Ravage );

	}
}

public class BlightConfig {

	public ITokenAddedArgs BlightFromCardTrigger;

	public bool ShouldCascade = true;
	public bool TakeFromCard = true;
	public bool DestroyPresence = true;
}