namespace SpiritIsland.Basegame;

public class PushPresenceFromOcean : SpiritAction {

	public PushPresenceFromOcean():base( "Push Presence from Ocean" ) { }

	public override async Task ActAsync( Spirit self ) {

		var pushSpaces = self.Presence.Lands
			.Where( p => p.IsOcean )
			.ToArray();

		foreach(var space in pushSpaces )
			await PushPresence( self, space );
	}

	static async Task PushPresence( Spirit self, Space from ) {
		var srcTokens = from.Tokens;
		var presenceTokens = self.Presence.TokensDeployedOn( srcTokens ).On(from).ToArray();
		var token = await self.SelectAsync( new A.SpaceToken( "Select presence to push", presenceTokens, Present.AutoSelectSingle ) );

		// #pushpresence
		Space destination = await self.SelectAsync( A.Space.ToPushPresence( from, srcTokens.Adjacent.Downgrade(), Present.Always, token.Token ) );

		// apply...
		await token.MoveTo( destination );
	}

}