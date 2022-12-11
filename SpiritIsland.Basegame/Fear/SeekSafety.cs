namespace SpiritIsland.Basegame;

public class SeekSafety : IFearCard {

	public const string Name = "Seek Safety";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }


	[FearLevel( 1, "Each player may Push 1 Explorer into a land with more Town / City than the land it came from." )]
	public async Task Level1( GameCtx ctx ) {

		Dictionary<Space, int> buildingCounts = ctx.GameState.AllActiveSpaces
			.ToDictionary(
				s=>s.Space,
				s=>s.TownsAndCitiesCount()
			);

		// Each player may
		foreach(var spiritCtx in ctx.Spirits)
			// Push 1 Explorer into a land with more Town / City than the land it came from.
			await PushExplorerIntoSpaceWithMoreTownsOrCities( spiritCtx, buildingCounts );

	}

	static async Task PushExplorerIntoSpaceWithMoreTownsOrCities( SelfCtx ctx, Dictionary<Space, int> buildingCounts ) {

		Space[] GetNeighborWithMoreBuildings( SpaceState s ) => s.Adjacent.Select(s=>s.Space).Where( n => buildingCounts[n] > buildingCounts[s.Space] ).ToArray();
		bool HasNeighborWithMoreBuildings(SpaceState s) => GetNeighborWithMoreBuildings(s).Any();

		// Select Source
		var sourceOptions = ctx.GameState.AllActiveSpaces
			.Where(s=>s.Has(Invader.Explorer) && HasNeighborWithMoreBuildings(s))
			.Select(s=>s.Space)
			.ToArray();
		if(sourceOptions.Length==0) return;

		Space source = await ctx.Decision( new Select.Space( "Fear: Select land to push explorer from into more towns/cities", sourceOptions,Present.Done));
		if(source==null) return; // continue => next spirit, break/return => no more spirits

		// Push
		await ctx.Target(source).PushUpTo(1,Invader.Explorer);
	}

	[FearLevel( 2, "Each player may Gather 1 Explorer into a land with Town / City, or Gather 1 Town into a land with City." )]
	public async Task Level2( GameCtx ctx ) {
		var gs = ctx.GameState;
		foreach(var spiritCtx in ctx.Spirits) {
			var options = gs.AllActiveSpaces
				.Where( s => s.HasAny(Invader.Town,Invader.City) )
				.Select(s=>s.Space)
				.ToArray();
			if(options.Length == 0) break;
			var dest = await spiritCtx.Decision( new Select.Space( "Select space to gather town to city OR explorer to town", options, Present.Always));
			var destCtx = spiritCtx.Target(dest);
			var grp = destCtx.Tokens;
			var invadersToGather = new List<TokenClass>();
			if(grp.Has(Invader.City)) invadersToGather.Add( Invader.Town );
			if(grp.Has(Invader.Town)) invadersToGather.Add( Invader.Explorer );
			TokenClass[] invadersToGatherArray = invadersToGather.ToArray();

			await destCtx.GatherUpTo(1, invadersToGatherArray);
		}
	}

	[FearLevel( 3, "Each player may remove up to 3 Health worth of Invaders from a land without City." )]
	public async Task Level3( GameCtx ctx ) {
		var gs = ctx.GameState;
		foreach(SelfCtx spirit in ctx.Spirits) {

			var options = gs.AllActiveSpaces
				.Where( s => s.HasInvaders() && !s.Has(Invader.City) )
				.Select( s => s.Space )
				.ToArray();
			if(options.Length==0) return;
			var target = await spirit.Decision( new Select.Space( "Select space to remove 3 health of invaders", options, Present.Always));
			var sCtx = spirit.Target(target);

			if(sCtx.Tokens.Has(Invader.City))
				await sCtx.RemoveInvader(Invader.City);
			else
				await sCtx.RemoveInvader(Invader.Explorer);
				await sCtx.RemoveInvader(Invader.Explorer);
				await sCtx.RemoveInvader(Invader.Explorer);
		}
	}

}