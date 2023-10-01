namespace SpiritIsland.FeatherAndFlame;

class OpenTheWays : IActionFactory {

	static public SpecialRule Rule = new SpecialRule( "Open the Ways", "You may make up to two of your lands adjacent at a time.  You may change which lands are adjacent once between Actions." );

	#region IActionFactory

	public string Name => "Open the Ways";

	public string Text => Name;

	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => true;

	public async Task ActivateAsync( SelfCtx ctx ) {
		if(ctx.Self is not FinderOfPathsUnseen finder) return;

		var options = ctx.Self.Presence.Spaces.Tokens().ToList();
		// Select 2 space to link
		var end0 = (await ctx.SelectSpace( "Select 1st space to make adjacent", options.Downgrade() )).Tokens;
		options.Remove( end0 );
		var end1 = (await ctx.SelectSpace( "Select 2nd space to make adjacent", options.Downgrade() )).Tokens;

		// Remove old
		finder.GatewayToken?.RemoveSelf();

		// Add new
		finder.GatewayToken = new GatewayToken( finder.Presence.Token, end0, end1 );
	}

	#endregion

}

