namespace SpiritIsland.NatureIncarnate;

public class ReplacePresenceWithIncarna : SpiritAction {

	public ReplacePresenceWithIncarna():base( "Replace Presence with Incarna" ) { }

	public override async Task ActAsync( Spirit self ) {
		// Remove presnece
		var spaceToken = await self.SelectAsync( A.SpaceTokenDecision.OfDeployedPresence( "Select presence to replace with Incarna.", self, Present.Done ) );
		if(spaceToken is null ) return;

		await spaceToken.Destroy();

		// Move/Place Incarna
		await PlaceIncarnaOn( self, spaceToken.Space.SpaceSpec );
	}

	static async Task PlaceIncarnaOn( Spirit spirit, SpaceSpec space ) { // duplicate
		var incarna = spirit.Incarna;
		if(incarna.IsPlaced)
			await incarna.Space.RemoveAsync( incarna, 1 );
		
		await space.ScopeSpace.AddAsync( incarna, 1 );
	}
}
