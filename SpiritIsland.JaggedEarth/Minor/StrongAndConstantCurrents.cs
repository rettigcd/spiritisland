namespace SpiritIsland.JaggedEarth;

public class StrongAndConstantCurrents{ 
	[MinorCard("Strong and Constant Currents",0,Element.Sun,Element.Water,Element.Earth),Fast,FromPresence(0, Target.Coastal)]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// If you hvae 2 water: 
		if(await ctx.YouHave("2 water" )) {
			// You may do both.
			await PushExporerTownToAdjacenCoastland.Execute(ctx);
			await MoveUpTo2DahanToAnotherCostal.Execute(ctx);
		} else
			await ctx.SelectActionOption( 
				PushExporerTownToAdjacenCoastland, 
				MoveUpTo2DahanToAnotherCostal
			);
	}

	#region Push Explorer / Town Option

	static public SpaceAction PushExporerTownToAdjacenCoastland => new SpaceAction( $"Push explorer/town to an adjacent Coastal land", PushToAdjacenCostalAction );

	static Task PushToAdjacenCostalAction( TargetSpaceCtx ctx ) {
		return ctx.Pusher
			.FilterDestinations( a=>a.Space.IsCoastal )
			.AddGroup(1, Invader.Explorer, Invader.Town)
			.MoveUpToN();
	}

	#endregion

	#region Move Dahan

	static public SpaceAction MoveUpTo2DahanToAnotherCostal => new SpaceAction("Move up to 2 Dahan between target land and one other Costal land.", MoveDahanAction );

	// Move up to 2 between target land and one other costal Land.
	static async Task MoveDahanAction( TargetSpaceCtx ctx ) {
		int count = 2;
		var costalCtxs = ctx.GameState.AllActiveSpaces
			.Select( s=>ctx.Target(s.Space) )
			.Where( x => x.IsCoastal )
			.Select( x=> x.Tokens )
			.ToArray();

		while(0 < count) {

			var costalWithDahan = costalCtxs
				.SelectMany( x => x.Dahan.Keys.Select(k=>new SpaceToken(x.Space,k)))
				.ToArray();

			// From
			var selected = await ctx.Decision( 
				new Select.TokenFromManySpaces( "Select Dahan to move to/from"+ctx.Space, costalWithDahan, Present.Done ) {
					AdjacentInfo = new Select.AdjacentInfo {
						Central = ctx.Space,
						Adjacent = costalWithDahan.Select( s => s.Space ).Distinct().ToArray(),
						Direction = Select.AdjacentDirection.Incoming
					}
				});

			// To:
			var destination = (selected.Space != ctx.Space) ? ctx.Space
				: await ctx.Decision( Select.Space.PushToken( selected.Token, selected.Space, costalCtxs, Present.Always ) );

			await ctx.Move( selected.Token, selected.Space, destination );

			count--;
		}
	}

	#endregion

}