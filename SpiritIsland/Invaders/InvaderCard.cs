namespace SpiritIsland;

public class InvaderCard : IOption, IInvaderCard {

	public static InvaderCard Stage1( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 1 );
	public static InvaderCard Stage2( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 2 );
	public static InvaderCard Stage2Costal() => new InvaderCard( new CostalFilter(), 2 );
	public static InvaderCard Stage3(Terrain t1,Terrain t2) => new InvaderCard( new DoubleTerrainFilter( t1, t2 ), 3);

	public bool Flipped { get; set; } = false;
	public bool HoldBack { get; set; } = false;
	public bool Skip { get; set; } = false;

	public int InvaderStage { get; }

	public bool Matches( Space space ) => Filter.Matches( space );
	public bool Matches( SpaceState space ) => Filter.Matches( space.Space );


	#region Constructors

	public InvaderCard( SpaceFilter filter, int invaderStage ) {
		Filter = filter;
		InvaderStage = invaderStage;
	}
	protected InvaderCard( InvaderCard orig ) {
		Filter = orig.Filter;
		InvaderStage = orig.InvaderStage;
	}

	#endregion

	protected bool HasEscalation => InvaderStage == 2 && Filter.Text != "Costal";
	public SpaceFilter Filter { get; }
	public string Text => (HasEscalation ? "2" : "") + Filter.Text;

	public async Task Ravage( GameState gs ) {
		gs.Log( new InvaderActionEntry( "Ravaging:" + Text ) );
		var ravageSpaces = gs.AllActiveSpaces.Where( Matches ).ToList();

		// Modify / Adjust
		var actionId = Guid.NewGuid();
		await gs.PreRavaging?.InvokeAsync( new RavagingEventArgs( gs, actionId ) { Spaces = ravageSpaces } );

		// find ravage spaces that have invaders
		InvaderBinding[] ravageGroups = ravageSpaces
			.Select( x=>gs.Invaders.On(x.Space,actionId) )
			.Where( group => group.Tokens.HasInvaders() )
			.Cast<InvaderBinding>()
			.ToArray();

		foreach(InvaderBinding invaderBinding in ravageGroups) {
			var @event = await new RavageAction( gs, invaderBinding ).Exec();
			gs.Log( new InvaderActionEntry( @event.ToString() ) );
		}

	}

	#region Build stuff

	public async Task Build( GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Building:" + Text ) );

		// Find spaces that match Card's Terrain
		var spacesGettingBuildTokens = gameState.AllActiveSpaces
			.Where( tokens => Matches(tokens.Space) ) // matches 
			.Where( tokens => ShouldBuildOnSpace( tokens ) )
			.ToArray();

		foreach(var tokens in spacesGettingBuildTokens)
			tokens.Adjust( TokenType.DoBuild, 1 );

		// Find spaces with Build Tokens
		var spacesWithBuildTokens = gameState.AllActiveSpaces
			.Where( tokens => tokens[TokenType.DoBuild] > 0 )
			.OrderBy( tokens => tokens.Space.Label )
			.ToArray();

		// Modify
		await gameState.PreBuilding.InvokeAsync( new BuildingEventArgs(
			gameState,
			spacesWithBuildTokens.ToArray()
		) );

		// report spaces that did not get built on.
		var noBuildsSpaceNames = spacesGettingBuildTokens.Except( spacesWithBuildTokens ).Select( x => x.Space.Text ).ToArray();
		if(noBuildsSpaceNames.Length > 0)
			gameState.Log( new InvaderActionEntry( "No build due to no invaders on: " + string.Join( ", ", noBuildsSpaceNames ) ) );

		// Do Build on spaces with build tokens
		BuildEngine buildEngine = gameState.GetBuildEngine();
		foreach(SpaceState tokens in spacesWithBuildTokens)
			await BuildIn1Space( gameState, buildEngine, tokens );

	}

	protected virtual bool ShouldBuildOnSpace( SpaceState tokens ) => tokens.HasInvaders();

	protected virtual async Task BuildIn1Space( GameState gameState, BuildEngine buildEngine, SpaceState tokens ) {
		string buildResult = await buildEngine.Exec( tokens, gameState );
		gameState.Log( new InvaderActionEntry( tokens.Space.Label + ": " + buildResult ) );
	}

	#endregion Build

	#region Explore methods

	public virtual async Task Explore( GameState gs ) {
		SpaceState[] tokenSpacesToExplore = await PreExplore( gs );
		await DoExplore( gs, tokenSpacesToExplore );
	}

	protected async Task<SpaceState[]> PreExplore( GameState gs ) {
		Flipped = true;

		gs.Log( new InvaderActionEntry( "Exploring:" + Text ) );

		// Modify
		static bool IsExplorerSource( SpaceState space ) => space.Space.IsOcean || space.HasAny( Invader.Town, Invader.City );
		var args = new ExploreEventArgs( gs,
			gs.AllActiveSpaces.Where( IsExplorerSource ),
			gs.AllActiveSpaces.Where( Matches )
		);
		await gs.PreExplore.InvokeAsync( args );

		return args.WillExplore( gs ).ToArray();
	}

	protected static async Task DoExplore( GameState gs, SpaceState[] tokenSpacesToExplore ) {
		foreach(var exploreTokens in tokenSpacesToExplore)
			await ExploreSingleSpace( exploreTokens, gs, Guid.NewGuid() );
	}

	static protected async Task ExploreSingleSpace( SpaceState tokens, GameState gs, Guid actionId ) {
		// only gets called when explorer is actually going to explore
		var wilds = tokens.Wilds;
		if(wilds.Count == 0) {
			gs.Log( new SpaceExplored( tokens.Space ) );
			await tokens.AddDefault( Invader.Explorer, 1, actionId, AddReason.Explore );
		} else
			await wilds.Bind(actionId).Remove( 1, RemoveReason.UsedUp );
	}

	#endregion

}

class SingleTerrainFilter : SpaceFilter {
	public SingleTerrainFilter( Terrain terrain ) {
		this.terrain = terrain;
		this.Text = terrain.ToString()[..1];
	}
	public bool Matches( Space space ) => space.Is( terrain );
	public string Text { get; }
	readonly Terrain terrain;
}

class DoubleTerrainFilter : SpaceFilter {
	public DoubleTerrainFilter( Terrain t1, Terrain t2 ) {
		terrain1 = t1;
		terrain2 = t2;
		Text = t1.ToString()[..1] + "+" + t2.ToString()[..1];
	}
	public bool Matches( Space space ) => space.IsOneOf( terrain1, terrain2 );
	public string Text { get; }
	readonly Terrain terrain1, terrain2;
}


class CostalFilter : SpaceFilter {
	public bool Matches( Space space ) => space.IsCoastal;
	public string Text => "Costal";
}
