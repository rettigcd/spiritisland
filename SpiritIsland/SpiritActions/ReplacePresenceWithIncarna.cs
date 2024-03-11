namespace SpiritIsland.NatureIncarnate;

public class ReplacePresenceWithIncarna : SpiritAction {

	public ReplacePresenceWithIncarna():base( "Replace Presence with Incarna" ) { }

	public override async Task ActAsync( Spirit self ) {
		// Remove presnece
		var spaceToken = await self.SelectAsync( A.SpaceToken.OfDeployedPresence( "Select presence to replace with Incarna.", self, Present.Done ) );
		if(spaceToken == null ) return;

		await spaceToken.Destroy();

		// Move/Place Incarna
		await PlaceIncarnaOn( self, spaceToken.Space );
	}

	static async Task PlaceIncarnaOn( Spirit spirit, Space space ) { // duplicate
		var incarna = spirit.Incarna;
		if(incarna.IsPlaced)
			await incarna.Space.RemoveAsync( incarna, 1 );
		
		await space.ScopeTokens.AddAsync( incarna, 1 );
	}
}
