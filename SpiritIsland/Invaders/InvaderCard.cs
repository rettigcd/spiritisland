namespace SpiritIsland;

public abstract class InvaderCard : IOption, IInvaderCard {

	public static InvaderCard Stage1( Terrain t1 ) => new SingleTerrainInvaderCard( t1, false );
	public static InvaderCard Stage2( Terrain t1 ) => new SingleTerrainInvaderCard( t1, true );
	public static InvaderCard Stage2Costal() => new CostalInvaderCard();
	public static InvaderCard Stage3(Terrain t1,Terrain t2) => new DoubleTerrainInvaderCard(t1, t2);

	public bool Flipped { get; set; } = false;
	public bool HoldBack { get; set; } = false;
	public bool Skip { get; set; } = false;

	public int InvaderStage { get; }

	public string Text { get; }

	public bool Escalation { get; }

	public abstract bool Matches( Space space );

	#region Constructors

	protected InvaderCard( string text, int invaderStage, bool escalation ) {
		Text = text;
		InvaderStage = invaderStage;
		Escalation = escalation;
	}

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

	public async Task Explore( GameState gs ) {
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

class SingleTerrainInvaderCard : InvaderCard {
	public SingleTerrainInvaderCard( Terrain terrain, bool escalation )
		: base(
			text: escalation ? "2" + terrain.ToString()[..1].ToLower() : terrain.ToString()[..1],
			invaderStage: escalation ? 2 : 1,
			escalation
		) {
		if(terrain == Terrain.Ocean) throw new ArgumentException( "Can't invade oceans" );
		this.terrain = terrain;
	}
	public override bool Matches( Space space ) => space.Is( terrain );

	readonly Terrain terrain;
}

class CostalInvaderCard : InvaderCard {
	public CostalInvaderCard() : base( "Costal", 2, false ) { }
	public override bool Matches( Space space ) => space.IsCoastal;
}

class DoubleTerrainInvaderCard : InvaderCard {
	readonly Terrain terrain1;
	readonly Terrain terrain2;
	public DoubleTerrainInvaderCard( Terrain t1, Terrain t2 )
		: base( t1.ToString()[..1] + "+" + t2.ToString()[..1], 3, false ) {
		this.terrain1 = t1;
		this.terrain2 = t2;
	}
	public override bool Matches( Space space ) => space.IsOneOf( terrain1, terrain2 );
}
