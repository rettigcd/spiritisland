namespace SpiritIsland.NatureIncarnate;

public class ReplacePresenceWithIncarna : SpiritAction {

	public ReplacePresenceWithIncarna():base( "ReplacePresenceWithIncarna" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		// Remove presnece
		var spaceToken = await ctx.Self.Gateway.Select( new A.SpaceToken( "Select presence to replace with Incarna.", ctx.Self.Presence.Deployed, Present.Done ) );
		if(spaceToken == null ) return;

		await spaceToken.Destroy();

		// Move/Place Incarna
		await PlaceIncarnaOn( ctx.Self, spaceToken.Space );
	}

	static async Task PlaceIncarnaOn( Spirit spirit, Space space ) { // duplicate
		if( spirit.Presence is not IHaveIncarna ihi) return;
		var incarna = ihi.Incarna;
		if(incarna.Space != null)
			await incarna.Space.Remove( incarna, 1 );

		await space.Tokens.Add( incarna, 1 );
	}
}
