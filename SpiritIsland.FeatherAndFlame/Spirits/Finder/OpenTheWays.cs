namespace SpiritIsland.FeatherAndFlame;

class OpenTheWays : IActionFactory {

	static public SpecialRule Rule = new SpecialRule( "Open the Ways", "You may make up to two of your lands adjacent at a time.  You may change which lands are adjacent once between Actions." );

	#region IActionFactory

	public string Title => "Open the Ways";

	public string Text => Title;

	public bool CouldActivateDuring( Phase speed, Spirit spirit ) => true;

	public async Task ActivateAsync( Spirit self ) {
		if(self is not FinderOfPathsUnseen finder) return;

		var options = self.Presence.Lands.ToList();
		// Select 2 space to link
		var end0 = (await self.SelectSpaceAsync( "Select 1st space to make adjacent", options,Present.Always ))!;
		options.Remove( end0 );
		var end1 = (await self.SelectSpaceAsync( "Select 2nd space to make adjacent", options,Present.Always ))!;

		// Remove old
		finder.GatewayToken?.RemoveSelf();

		// Add new
		finder.GatewayToken = new GatewayToken( finder.Presence.Token, end0, end1 );
	}


	#endregion

}

