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
			.FilterDestinations( a=>a.IsCoastal )
			.AddGroup(1, Invader.Explorer, Invader.Town)
			.MoveUpToN();
	}

	#endregion

	#region Move Dahan

	static public SpaceAction MoveUpTo2DahanToAnotherCostal => new SpaceAction("Move up to 2 Dahan between target land and one other Costal land.", MoveDahanAction );

	static async Task MoveDahanAction( TargetSpaceCtx ctx ) {
		int count = 2;
		while(0 < count) {
			var costalCtxs = ctx.AllSpaces.Select( ctx.Target ).Where( x=>x.IsCoastal ).ToArray();
			var costalWithDahan = costalCtxs.Where( x=>x.Dahan.Any ).ToArray();
			var costal = costalCtxs.Select(x=>x.Space).ToArray();

			await ctx.SelectActionOption(
				new SpaceAction($"Move Dahan IN TO "+ ctx.Space.Label, ctx => ctx.MoveTokenIn( TokenType.Dahan, 100, Target.Coastal)).FilterOption( costalWithDahan.Length>0 ),
				new SpaceAction($"Move Dahan OUT OF "+ ctx.Space.Label, ctx => ctx.MoveTokensOut(1, TokenType.Dahan, 100, Target.Coastal) ).Matches( x => x.Dahan.Any )
			);

			count--;
		}
	}

	#endregion

}