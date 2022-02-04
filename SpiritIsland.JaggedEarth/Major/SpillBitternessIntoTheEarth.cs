namespace SpiritIsland.JaggedEarth;

public class SpillBitternessIntoTheEarth {

	[MajorCard("Spill Bitterness into the Earth",5,Element.Fire,Element.Water,Element.Earth), Fast, FromPresence(0)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 6 damage.
		await ctx.DamageInvaders( 6 );

		// Add 2 badlands/strife
		var addBadlandsOrStrife = Cmd.Pick1( "Add badlands/strife", Cmd.AddBadlands(1), Cmd.AddStrife(1) );
		await addBadlandsOrStrife.Repeat(2).Execute( ctx );

		// and 1 blight.
		await ctx.AddBlight();

		await TakeActionInUpToNLands( ctx
			// In up to 3 adjacent lands with blight
			, 3, ctx.Adjacent.Where( s => ctx.Target( s ).HasBlight )
			// add 1 badland/strife.
			, addBadlandsOrStrife
		);

		// if you have 3 fire 3 water:
		if(await ctx.YouHave( "3 fire,3 water" ))
			await TakeActionInUpToNLands( ctx
				// in up to 3 adjacent lands,
				, 3, ctx.Adjacent
				// 1 damage to each invader.
				, new SpaceAction("1 damage to each invader", ctx => ctx.DamageEachInvader(1) )
			);

	}

	static async Task TakeActionInUpToNLands( SelfCtx ctx, int adjCount, IEnumerable<Space> spaces, ActionOption<TargetSpaceCtx> action ) {
		List<Space> options = spaces.ToList();
		while(adjCount-- > 0 && options.Count > 0) {
			var space = await ctx.Decision( new Select.Space( $"{action.Description} ({adjCount + 1} remaining)", options, Present.Done ) );
			if(space == null) break;
			await action.Execute( ctx.Target(space) );
			options.Remove( space );
		}
	}

}