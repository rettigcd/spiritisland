namespace SpiritIsland.NatureIncarnate;

public class ReplacePresenceWithIncarna : GrowthActionFactory {
	public override async Task ActivateAsync( SelfCtx ctx ) {
		// Remove presnece
		var spaceToken = await ctx.Self.PickPresenceToDestroy( "Select presence to replace with Incarna." );

		// Move/Place Incarna
		await PlaceIncarnaOn( ((ToweringRootsOfTheJungle)ctx.Self).Incarna, spaceToken.Space );
	}

	private static async Task PlaceIncarnaOn( ToweringRootsIncarna incarna, Space space ) {
		if(incarna.Space != null)
			await incarna.Space.Remove( incarna, 1 );

		await space.Tokens.Add( incarna, 1 );
	}
}
