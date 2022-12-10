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

	public bool MatchesCard( Space space ) => Filter.Matches( space );
	public bool MatchesCard( SpaceState space ) => Filter.Matches( space.Space );


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
		var ravageSpacesMatchingCard = gs.AllActiveSpaces.Where( MatchesCard ).ToList();

		// find ravage spaces that have invaders
		var ravageSpacesWithInvaders = ravageSpacesMatchingCard
			.Where( tokens => tokens.HasInvaders() )
			.ToArray();

		// Add Ravage tokens to spaces with invaders
		foreach(var s in ravageSpacesWithInvaders)
			s.Adjust( TokenType.DoRavage, 1 );

		// get spaces with just-added Ravages + any previously added ravages
		var spacesWithDoRavage = gs.AllActiveSpaces
			.Where( ss=>ss[TokenType.DoRavage] > 0)
			.ToArray();

		foreach(var ravageSpace in spacesWithDoRavage) {
			int ravageCount = ravageSpace[TokenType.DoRavage];
			ravageSpace.Init( TokenType.DoRavage, 0);

			while(0 < ravageCount--)
				await new RavageAction( gs, ravageSpace ).Exec();

		}

	}

	#region Build stuff

	public async Task Build( GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Building:" + Text ) );

		// Add BuildTokens To Matching Spaces
		AddBuildTokensMatchingCard( gameState );

		// Scan for all Build Tokens - both Card-Build-Spaces plus any pre-existing DoBuilds
		// ** May contain more than just Normal Build, due to rule/power that added extra ones.
		var matchingSpacesWithBuildTokens = gameState.AllActiveSpaces
			.Where( tokens => 0 < tokens[TokenType.DoBuild] )
			.OrderBy( tokens => tokens.Space.Label )
			.ToArray();

		// Do Builds on each space
		foreach(var spaceState in matchingSpacesWithBuildTokens)
			await BuildIn1Space( gameState, spaceState );

	}

	void AddBuildTokensMatchingCard( GameState gameState ) {
		var cardDependentBuildSpaces = gameState.AllActiveSpaces
			.Where( MatchesCard )           // space matches card
			.ToArray();
		var spacesMatchingCardCriteria = cardDependentBuildSpaces
			.Where( ShouldBuildOnSpace )    // usually because it has invaders on it
			.ToArray();
		foreach(SpaceState tokens in spacesMatchingCardCriteria)
			tokens.Adjust( TokenType.DoBuild, 1 );

		// log any spaces that look like they should get built on but didn't
		var noBuildsSpaceNames = cardDependentBuildSpaces   // Space that should be build on
			.Except( spacesMatchingCardCriteria )    // Spaces that we are actually building on.
			.Select( x => x.Space.Text )
			.ToArray();
		if(0 < noBuildsSpaceNames.Length)
			gameState.Log( new InvaderActionEntry( "No build due to no invaders on: " + string.Join( ", ", noBuildsSpaceNames ) ) );
	}

	// Overriden by France
	protected virtual async Task BuildIn1Space( GameState gameState, SpaceState tokens ) {
		BuildEngine buildEngine = gameState.GetBuildEngine( tokens );
		string buildResult = await buildEngine.DoBuilds();
		gameState.Log( new InvaderActionEntry( tokens.Space.Label + ": " + buildResult ) );
	}

	// Overriden by England
	protected virtual bool ShouldBuildOnSpace( SpaceState tokens ) => tokens.HasInvaders();

	#endregion Build

	#region Explore methods

	public virtual async Task Explore( GameState gs ) {
		SpaceState[] tokenSpacesToExplore = await PreExplore( gs );
		await DoExplore( gs, tokenSpacesToExplore );
	}

	protected async Task<SpaceState[]> PreExplore( GameState gs ) {

		gs.Log( new InvaderActionEntry($"Flipping {this.Text} - was {this.Flipped}") );

		Flipped = true;

		gs.Log( new InvaderActionEntry( "Exploring:" + Text ) );

		// Modify
		static bool IsExplorerSource( SpaceState space ) => space.Space.IsOcean || space.HasAny( Invader.Town, Invader.City );

		var sources = gs.AllActiveSpaces
			.Where( IsExplorerSource )
			.Where( ss => !ss.Keys.OfType<ISkipExploreFrom>().Any() )
			.ToHashSet();


		var exploreRoutes = gs.AllActiveSpaces.Where( MatchesCard )
			.SelectMany( dst => dst.Range( 1 )
				.Where( sources.Contains )
				.Select( src => new ExploreRoute { Source = src, Destination = dst } )
			)
			.OrderBy( route => route.Destination.Space.Label )
			.ThenBy( route => route.Source.Space.Label )
			.ToArray();


		var spacesWeExplore = exploreRoutes
			.Where( rt => rt.IsValid )
			.Select( rt => rt.Destination )
			.Distinct()
			.OrderBy( x => x.Space.Label )
			.ToArray();


		foreach(var x in spacesWeExplore)
			x.Adjust( TokenType.DoExplore, 1 );

		return gs.AllActiveSpaces
			.Where( x=>x[TokenType.DoExplore]>0)
			.ToArray();
	}

	protected static async Task DoExplore( GameState gs, SpaceState[] tokenSpacesToExplore ) {
		foreach(var exploreTokens in tokenSpacesToExplore) {
			int exploreCount = exploreTokens[TokenType.DoExplore];
			exploreTokens.Init(TokenType.DoExplore,0);
			while(0 < exploreCount--)
				await ExploreSingleSpace( exploreTokens, gs, gs.StartAction( ActionCategory.Invader ) );
		}
	}

	static protected async Task ExploreSingleSpace( SpaceState tokens, GameState gs, UnitOfWork actionId ) {
		await using var uow = gs.StartAction( ActionCategory.Invader );
		var ctx = new GameCtx( gs, uow );
		foreach( var stopper in tokens.Keys.OfType<ISkipExploreTo>().ToArray() )
			if( await stopper.Skip(ctx,tokens) )
				return;

		gs.Log( new SpaceExplored( tokens.Space ) );
		await tokens.AddDefault( Invader.Explorer, 1, actionId, AddReason.Explore );
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
