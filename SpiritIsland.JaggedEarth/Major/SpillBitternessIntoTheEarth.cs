namespace SpiritIsland.JaggedEarth;

public class SpillBitternessIntoTheEarth {

	[MajorCard("Spill Bitterness Into the Earth",5,Element.Fire,Element.Water,Element.Earth), Fast, FromPresence(0)]
	[Instructions( "6 Damage. Add 2 Badlands / Strife and 1 Blight. In up to 3 adjacent lands with Blight, add 1 Badlands / Strife. -If you have- 3 Fire, 3 Water: In up to 3 adjacent lands, 1 Damage to each Invader." ), Artist( Artists.MoroRogers )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 6 damage.
		await ctx.DamageInvaders( 6 );

		// Add 2 badlands/strife
		var addBadlandsOrStrife = Cmd.Pick1( "Add badlands/strife", Cmd.AddBadlands(1), Cmd.AddStrife(1) );
		await addBadlandsOrStrife.Repeat(2).ActAsync( ctx );

		// and 1 blight.
		await ctx.AddBlight(1);

		await TakeActionInUpToNLands( ctx.Self
			// In up to 3 adjacent lands with blight
			, 3, ctx.Adjacent.Where( s => s.Blight.Any )
			// add 1 badland/strife.
			, addBadlandsOrStrife
		);

		// if you have 3 fire 3 water:
		if(await ctx.YouHave( "3 fire,3 water" ))
			await TakeActionInUpToNLands( ctx.Self
				// in up to 3 adjacent lands,
				, 3, ctx.Adjacent
				// 1 damage to each invader.
				, new SpaceAction("1 damage to each invader", ctx => ctx.DamageEachInvader(1) )
			);

	}

	static async Task TakeActionInUpToNLands( Spirit self, int adjCount, IEnumerable<Space> spaces, BaseCmd<TargetSpaceCtx> action ) {
		List<Space> options = spaces.ToList();
		while(adjCount-- > 0 && options.Count > 0) {
			var space = await self.SelectAsync( new A.SpaceDecision( $"{action.Description} ({adjCount + 1} remaining)", options, Present.Done ) );
			if(space == null) break;
			await action.ActAsync( self.Target(space) );
			options.Remove( space );
		}
	}

}