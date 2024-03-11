namespace SpiritIsland.Basegame;

public class PushPresenceFromOcean : SpiritAction {

	public PushPresenceFromOcean():base( "Push Presence from Ocean" ) { }

	public override async Task ActAsync( Spirit self ) {

		var pushSpaces = self.Presence.Lands
			.Where( p => p.Space.IsOcean )
			.ToArray();

		foreach( SpaceState space in pushSpaces )
			await PushPresence( self, space.Space );
	}

	static async Task PushPresence( Spirit self, Space from ) {
		var srcTokens = from.ScopeTokens;
		var presenceTokens = self.Presence.TokensDeployedOn( srcTokens ).OnScopeTokens1(from).ToArray();
		var token = await self.SelectAsync( new A.SpaceToken( "Select presence to push", presenceTokens, Present.AutoSelectSingle ) );

		// #pushpresence
		Space destination = await self.SelectAsync( A.Space.ToPushPresence( from, srcTokens.Adjacent.Downgrade(), Present.Always, token.Token ) );

		// apply...
		await token.MoveTo( destination.ScopeTokens );
	}

}