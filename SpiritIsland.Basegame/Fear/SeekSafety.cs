namespace SpiritIsland.Basegame;

public class SeekSafety : FearCardBase, IFearCard {

	public const string Name = "Seek Safety";
	public string Text => Name;

	[FearLevel( 1, "Each player may Push 1 Explorer into a land with more Town/City than the land it came from." )]
	public Task Level1( GameState ctx )
		=> Cmd.Describe<Spirit>( "Push 1 Explorer into a land with more Town/City than the land it came from", PushExplorerIntoSpaceWithMoreTownsOrCities )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player may Gather 1 Explorer into a land with Town/City, or Gather 1 Town into a land with City." )]
	public Task Level2( GameState ctx )
		=> new SpaceAction("gather 1 explorer / town into a land with bigger invader", GatherExplorerOrTown)
			.In().SpiritPickedLand().Which( Has.TownOrCity )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 3, "Each player may remove up to 3 Health worth of Invaders from a land without City." )]
	public Task Level3( GameState ctx )
		=> Cmd.RemoveUpToNHealthOfInvaders( 3 )
			.In().SpiritPickedLand().Which( Has.NoCity )
			.ForEachSpirit()
			.ActAsync( ctx );

	static async Task PushExplorerIntoSpaceWithMoreTownsOrCities( Spirit self ) {

		var gs = GameState.Current;
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

		Space source = await self.SelectAsync( new A.Space( "Fear: Select land to push explorer from into more towns/cities", sourceOptions, Present.Done ) );
		if(source == null) return; // continue => next spirit, break/return => no more spirits

		// Push
		await self.Target( source ).PushUpTo( 1, Human.Explorer );
	}

	static async Task GatherExplorerOrTown( TargetSpaceCtx destCtx ) {
		var invadersToGather = new List<ITokenClass>();
		if(destCtx.Tokens.Has( Human.City )) invadersToGather.Add( Human.Town );
		if(destCtx.Tokens.Has( Human.Town )) invadersToGather.Add( Human.Explorer );
		ITokenClass[] invadersToGatherArray = invadersToGather.ToArray();
		await destCtx.GatherUpTo( 1, invadersToGatherArray );
	}
	
}