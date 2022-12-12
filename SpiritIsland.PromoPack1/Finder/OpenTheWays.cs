namespace SpiritIsland.PromoPack1;

class OpenTheWays : IActionFactory {

	static public SpecialRule Rule = new SpecialRule( "Open the Ways", "You may make up to two of your lands adjacent at a time.  You may change which lands are adjacent once between Actions." );

	public OpenTheWays(FinderOfPathsUnseen self ) { this.self = self; }

	#region IActionFactory

	public string Name => "Open the Ways";

	public string Text => Name;

	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => true;

	public async Task ActivateAsync( SelfCtx ctx ) {
		var options = ctx.Presence.Spaces.ToList();
		// Select 2 space to link
		var end0 = (await ctx.SelectSpace( "Select 1st space to make adjacent", options )).Tokens;
		options.Remove( end0.Space );
		var end1 = (await ctx.SelectSpace( "Select 2nd space to make adjacent", options )).Tokens;

		Link( ref _token0, end0, end1 );
		Link( ref _token1, end1, end0 );
	}

	#endregion

	// !! Instead, we *could* do this by handling token-moved/removed events and remove link immediately
	public Task CheckPresenceAtBothEnds( UnitOfWork _ ) {
		if( IsLinked && MissingPresenceAtEitherEnd) {
			Unlink( ref _token0 );
			Unlink( ref _token1 );
		}
		return Task.CompletedTask;
	}

	bool IsLinked => _token0 != null && _token1 != null; // they should both be null or not-null

	bool MissingPresenceAtEitherEnd => 
			!self.Presence.IsOn( _token0.From ) 
		||	!self.Presence.IsOn( _token1.From );

	static void Link( ref GatewayToken trackedToken, SpaceState newTarget, SpaceState other ) {
		// Remove old
		Unlink( ref trackedToken );

		// Add new
		trackedToken = new GatewayToken( newTarget, other );
	}
	static void Unlink( ref GatewayToken trackedToken ) {
		trackedToken?.RemoveSelf();
		trackedToken = null;
	}

	readonly FinderOfPathsUnseen self;
	GatewayToken _token0;
	GatewayToken _token1;

}

