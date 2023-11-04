namespace SpiritIsland.JaggedEarth;

public class StrongAndConstantCurrents{ 

	[MinorCard("Strong and Constant Currents",0,Element.Sun,Element.Water,Element.Earth),Fast,FromPresence(0, Target.Coastal)]
	[Instructions( "Push 1 Explorer / Town to an adjacent Coastal land. -or- Move up to 2 Dahan between target land and one other Coastal land. -If you have- 2 Water: You may do both." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync(TargetSpaceCtx ctx){
		// If you hvae 2 water: 
		if(await ctx.YouHave("2 water" )) {
			// You may do both.
			await PushExporerTownToAdjacenCoastland.ActAsync(ctx);
			await MoveUpTo2DahanToAnotherCostal.ActAsync(ctx);
		} else
			await ctx.SelectActionOption( 
				PushExporerTownToAdjacenCoastland, 
				MoveUpTo2DahanToAnotherCostal
			);
	}

	#region Push Explorer / Town Option

	static public SpaceCmd PushExporerTownToAdjacenCoastland => new SpaceCmd( $"Push explorer/town to an adjacent Coastal land", PushToAdjacenCostalAction );

	static Task PushToAdjacenCostalAction( TargetSpaceCtx ctx ) {
		return ctx.Pusher
			.FilterDestinations( a=>a.Space.IsCoastal )
			.AddGroup(1, Human.Explorer_Town)
			.MoveUpToN();
	}

	#endregion

	#region Move Dahan

	static public SpaceCmd MoveUpTo2DahanToAnotherCostal => new SpaceCmd("Move up to 2 Dahan between target land and one other Costal land.", MoveDahanAction );

	// Move up to 2 between target land and one other coastal Land.
	static async Task MoveDahanAction( TargetSpaceCtx ctx ) {
		int count = 2;
		var coastalCtxs = GameState.Current.Spaces
			.Select( s=>ctx.Target(s.Space) )
			.Where( x => x.IsCoastal )
			.Select( x=> x.Tokens )
			.ToArray();

		while(0 < count) {

			var coastalWithDahan = coastalCtxs
				.SelectMany( x => x.Dahan.NormalKeys.Select(k=>new SpaceToken(x.Space,k)))
				.ToArray();

			// From
			var selected = await ctx.Decision( new Select.ASpaceToken( "Select Dahan to move to/from"+ctx.Space, coastalWithDahan, Present.Done, ctx.Space ));
			if(selected == null) break;

			// To:
			var destination = (selected.Space != ctx.Space) ? ctx.Space
				: await ctx.Decision( Select.ASpace.PushToken( selected.Token, selected.Space, coastalCtxs, Present.Always ) );

			await ctx.Move( selected.Token, selected.Space, destination );

			count--;
		}
	}

	#endregion

}