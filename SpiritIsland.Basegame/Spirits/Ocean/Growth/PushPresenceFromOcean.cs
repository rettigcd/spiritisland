namespace SpiritIsland.Basegame;

public class PushPresenceFromOcean : SpiritAction {

	public PushPresenceFromOcean():base( "Push Presence from Ocean" ) { }

	public override async Task ActAsync( Spirit self ) {

		var pushSpaces = self.Presence.Lands
			.Where( p => p.SpaceSpec.IsOcean )
			.ToArray();

		foreach( Space space in pushSpaces )
			await PushPresence( self, space );
	}

	static async Task PushPresence( Spirit self, Space srcTokens ) {

		var options = self.Presence.TokensDeployedOn(srcTokens).On(srcTokens).BuildMoves(x=>x.Space.Adjacent);
		Move move = await self.SelectAlways("Select presence to push",options);

		await move.Apply();

		//var presenceTokens = self.Presence.TokensDeployedOn( srcTokens ).On( srcTokens ).ToArray();
		//var token = await self.SelectAlways( "Select presence to push", presenceTokens, true );

		//// #pushpresence
		//Space destination = await self.SelectAlways( A.SpaceDecision.ToPushPresence(srcTokens, srcTokens.Adjacent, Present.Always, token.Token ) );

		//// apply...
		//await token.MoveTo( destination );
	}

}