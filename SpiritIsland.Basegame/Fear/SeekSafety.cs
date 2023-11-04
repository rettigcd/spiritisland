namespace SpiritIsland.Basegame;

public class SeekSafety : FearCardBase, IFearCard {

	public const string Name = "Seek Safety";
	public string Text => Name;

	[FearLevel( 1, "Each player may Push 1 Explorer into a land with more Town/City than the land it came from." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.Describe<SelfCtx>( "Push 1 Explorer into a land with more Town/City than the land it came from", PushExplorerIntoSpaceWithMoreTownsOrCities )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "Each player may Gather 1 Explorer into a land with Town/City, or Gather 1 Town into a land with City." )]
	public Task Level2( GameCtx ctx )
		=> new SpaceCmd("gather 1 explorer / town into a land with bigger invader", GatherExplorerOrTown)
			.In().SpiritPickedLand().Which( Has.TownOrCity )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 3, "Each player may remove up to 3 Health worth of Invaders from a land without City." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.RemoveHealthOfInvaders("Remove up to 3 Health of Invaders", _=>3 )
			.In().SpiritPickedLand().Which( Has.NoCity )
			.ForEachSpirit()
			.Execute( ctx );

	static async Task PushExplorerIntoSpaceWithMoreTownsOrCities( SelfCtx ctx ) {

		var gs= GameState.Current;
		Dictionary<Space, int> buildingCounts = gs.Spaces
			.ToDictionary(
				s => s.Space,
				s => s.TownsAndCitiesCount()
			);

		Space[] GetNeighborWithMoreBuildings( SpaceState s ) => s.Adjacent.Downgrade().Where( n => buildingCounts[n] > buildingCounts[s.Space] ).ToArray();
		bool HasNeighborWithMoreBuildings( SpaceState s ) => GetNeighborWithMoreBuildings( s ).Any();

		// Select Source
		var sourceOptions = gs.Spaces
			.Where( s => s.Has( Human.Explorer ) && HasNeighborWithMoreBuildings( s ) )
			.Downgrade()
			.ToArray();
		if(sourceOptions.Length == 0) return;

		Space source = await ctx.Decision( new Select.ASpace( "Fear: Select land to push explorer from into more towns/cities", sourceOptions, Present.Done ) );
		if(source == null) return; // continue => next spirit, break/return => no more spirits

		// Push
		await ctx.Target( source ).PushUpTo( 1, Human.Explorer );
	}

	static async Task GatherExplorerOrTown( TargetSpaceCtx destCtx ) {
		var invadersToGather = new List<IEntityClass>();
		if(destCtx.Tokens.Has( Human.City )) invadersToGather.Add( Human.Town );
		if(destCtx.Tokens.Has( Human.Town )) invadersToGather.Add( Human.Explorer );
		IEntityClass[] invadersToGatherArray = invadersToGather.ToArray();
		await destCtx.GatherUpTo( 1, invadersToGatherArray );
	}
	
}