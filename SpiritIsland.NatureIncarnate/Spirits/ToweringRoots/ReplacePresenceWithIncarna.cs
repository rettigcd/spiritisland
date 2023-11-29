namespace SpiritIsland.NatureIncarnate;

public class ReplacePresenceWithIncarna : SpiritAction {

	public ReplacePresenceWithIncarna():base( "ReplacePresenceWithIncarna" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		// Remove presnece
		var spaceToken = await ctx.Self.Select( A.SpaceToken.OfDeployedPresence( "Select presence to replace with Incarna.", ctx.Self, Present.Done ) );
		if(spaceToken == null ) return;

		await spaceToken.Destroy();

		// Move/Place Incarna
		await PlaceIncarnaOn( ctx.Self, spaceToken.Space );
	}

	static async Task PlaceIncarnaOn( Spirit spirit, Space space ) { // duplicate
		if( spirit.Presence is not IHaveIncarna ihi) return;
		var incarna = ihi.Incarna;
		if(incarna.Space != null)
			await incarna.Space.RemoveAsync( incarna, 1 );

		await space.Tokens.AddAsync( incarna, 1 );
	}
}
