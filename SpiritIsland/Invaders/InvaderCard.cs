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

	public Func<GameState, Task> Escalation;

	public bool Matches( Space space ) => Filter.Matches( space );

	#region Constructors

	public InvaderCard( SpaceFilter filter, int invaderStage ) {
		Filter = filter;
		InvaderStage = invaderStage;
	}
	protected bool HasEscalation => InvaderStage == 2 && Filter.Text != "Costal";
	public SpaceFilter Filter { get; }
	public string Text => (HasEscalation ? "2" : "") + Filter.Text;

	#endregion

	public async Task Ravage( GameState gs ) {
		gs.Log( new InvaderActionEntry( "Ravaging:" + Text ) );
		var ravageSpaces = gs.Island.AllSpaces.Where( Matches ).ToList();

		// Modify / Adjust
		var actionId = Guid.NewGuid();
		await gs.PreRavaging?.InvokeAsync( new RavagingEventArgs( gs, actionId ) { Spaces = ravageSpaces } );

		// find ravage spaces that have invaders
		InvaderBinding[] ravageGroups = ravageSpaces
			.Select( x=>gs.Invaders.On(x,actionId) )
			.Where( group => group.Tokens.HasInvaders() )
			.Cast<InvaderBinding>()
			.ToArray();

		foreach(InvaderBinding invaderBinding in ravageGroups) {
			var @event = await new RavageAction( gs, invaderBinding ).Exec();
			gs.Log( new InvaderActionEntry( @event.ToString() ) );
		}

	}

	protected virtual bool ShouldBuildOnSpace( TokenCountDictionary tokens, GameState _ ) {
		return tokens.HasInvaders();
	}

	public async Task Build( GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Building:" + Text ) );

		// Find spaces that match Card's Terrain
		var args = new BuildingEventArgs( gameState, new Dictionary<Space, BuildingEventArgs.BuildType>() ) {
			SpaceCounts = gameState.Island.AllSpaces.Where( Matches )
				.ToDictionary( s => s, grp => 1 )
				.ToCountDict(),
		};

		// Modify
		await gameState.PreBuilding.InvokeAsync( args );

		// Do build in each space
		BuildEngine buildEngine = gameState.GetBuildEngine();
		var buildGroups = args.SpaceCounts.Keys
			.OrderBy( x => x.Label )
			.Select( x => gameState.Tokens[x] )
			.ToArray();
		foreach(TokenCountDictionary tokens in buildGroups) {
			int count = args.SpaceCounts[tokens.Space];
			while(count-- > 0) {
				string buildResult = ShouldBuildOnSpace(tokens,gameState) 
					? await buildEngine.Exec( args, tokens, gameState )
					: "No invaders";
				gameState.Log( new InvaderActionEntry( tokens.Space.Label + ": gets " + buildResult ) );
			}
		}

	}

	public virtual async Task Explore( GameState gs ) {
		Flipped = true;

		gs.Log( new InvaderActionEntry( "Exploring:" + Text ) );

		// Modify
		bool IsExplorerSource( Space space ) { return space.IsOcean || gs.Tokens[space].HasAny( Invader.Town, Invader.City ); }
		var args = new ExploreEventArgs( gs,
			gs.Island.AllSpaces.Where( IsExplorerSource ),
			gs.Island.AllSpaces.Where( Matches )
		);
		await gs.PreExplore.InvokeAsync( args );

		var tokenSpacesToExplore = args.WillExplore( gs )
			.Select( x => gs.Tokens[x] )
			.ToArray();

		// Explore
		foreach(var exploreTokens in tokenSpacesToExplore)
			await ExploreSingleSpace( exploreTokens, gs, Guid.NewGuid() );

		if(Escalation != null)
			await Escalation( gs );
	}

	static async Task ExploreSingleSpace( TokenCountDictionary tokens, GameState gs, Guid actionId ) {
		// only gets called when explorer is actually going to explore
		var wilds = tokens.Wilds;
		if(wilds.Count == 0) {
			gs.Log( new InvaderActionEntry( tokens.Space + ":gains explorer" ) );
			await tokens.AddDefault( Invader.Explorer, 1, actionId, AddReason.Explore );
		} else
			await wilds.Bind(actionId).Remove( 1, RemoveReason.UsedUp );
	}

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
