namespace SpiritIsland.Basegame;

public class SeekSafety : FearCardBase, IFearCard {

	public const string Name = "Seek Safety";
	public string Text => Name;

	[FearLevel( 1, "Each player may Push 1 Explorer into a land with more Town/City than the land it came from." )]
	public override Task Level1( GameState ctx )
		=> PushExplorerIntoSpaceWithMoreTownsOrCities
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player may Gather 1 Explorer into a land with Town/City, or Gather 1 Town into a land with City." )]
	public override Task Level2( GameState ctx )
		=> new SpaceAction("gather 1 explorer / town into a land with bigger invader", GatherExplorerOrTown)
			.In().SpiritPickedLand().Which( Has.TownOrCity )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 3, "Each player may remove up to 3 Health worth of Invaders from a land without City." )]
	public override Task Level3( GameState ctx )
		=> Cmd.RemoveUpToNHealthOfInvaders( 3 )
			.In().SpiritPickedLand().Which( Has.NoCity )
			.ForEachSpirit()
			.ActAsync( ctx );

	static SpiritAction PushExplorerIntoSpaceWithMoreTownsOrCities => new SpiritAction("", PushExplorerIntoSpaceWithMoreTownsOrCities_Imp );
	static async Task PushExplorerIntoSpaceWithMoreTownsOrCities_Imp( Spirit self ) {

		var gs = GameState.Current;
		Dictionary<Space, int> buildingCounts = ActionScope.Current.Spaces
			.ToDictionary(
				s => s,
				s => s.TownsAndCitiesCount()
			);

		Space[] GetNeighborWithMoreBuildings( Space s ) => s.Adjacent.Where( n => buildingCounts[n] > buildingCounts[s] ).ToArray();
		bool HasNeighborWithMoreBuildings( Space s ) => GetNeighborWithMoreBuildings( s ).Length != 0;

		// Select Source
		Space[] sourceOptions = ActionScope.Current.Spaces
			.Where( s => s.Has( Human.Explorer ) && HasNeighborWithMoreBuildings( s ) )
			.ToArray();
		if(sourceOptions.Length == 0) return;

		Space? source = await self.Select( new A.SpaceDecision( "Fear: Select land to push explorer from into more towns/cities", sourceOptions, Present.Done ) );
		if(source is null) return; // continue => next spirit, break/return => no more spirits

		// Push
		int sourceCount=0;
		await source.SourceSelector
			.AddGroup( 1, Human.Explorer )
			.Track(s=>sourceCount = buildingCounts[s.Space] )
			.ConfigDestination( ds=>ds.FilterDestination( dst => sourceCount < buildingCounts[dst] ) )
			.PushUpToN( self );

		await self.Target( source ).PushUpTo( 1, Human.Explorer );
	}

	static async Task GatherExplorerOrTown( TargetSpaceCtx destCtx ) {
		var invadersToGather = new List<ITokenClass>();
		if(destCtx.Space.Has( Human.City )) invadersToGather.Add( Human.Town );
		if(destCtx.Space.Has( Human.Town )) invadersToGather.Add( Human.Explorer );
		ITokenClass[] invadersToGatherArray = [.. invadersToGather];
		await destCtx.GatherUpTo( 1, invadersToGatherArray );
	}
	
}