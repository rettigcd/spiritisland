namespace SpiritIsland;

public class InvaderCard : IOption, IInvaderCard {

	public static IInvaderCard Stage1( Terrain t1 ) => new InvaderCard( t1, false );
	public static IInvaderCard Stage2( Terrain t1 ) => new InvaderCard( t1, true );
	public static IInvaderCard Stage2Costal() => new CostalInvaderCard();
	public static IInvaderCard Stage3(Terrain t1,Terrain t2) => new Stage3InvaderCard(t1, t2);

	class CostalInvaderCard : InvaderCard {
		public CostalInvaderCard() : base( "Costal", 2, false ) { }
		public override bool Matches( Space space ) => space.IsCoastal;
	}

	class Stage3InvaderCard : InvaderCard {
		readonly Terrain t2;
		public Stage3InvaderCard( Terrain t1, Terrain t2 )
			: base( t1.ToString()[..1] + "+" + t2.ToString()[..1], 3, false ) {
			this.t1 = t1;
			this.t2 = t2;
		}
		public override bool Matches( Space space ) => space.IsOneOf( t1, t2 );
	}

	public int InvaderStage { get; }

	public string Text { get; }

	public bool Escalation { get; }

	public virtual bool Matches( Space space ) => space.Is( t1 );
	protected Terrain t1;

	#region Constructors

	/// <summary>
	/// Stage 1 or 2 constructor
	/// </summary>
	public InvaderCard( Terrain terrain, bool escalation = false ) {
		if(terrain == Terrain.Ocean) throw new ArgumentException( "Can't invade oceans" );
		InvaderStage = escalation ? 2 : 1;
		this.t1 = terrain;
		Text = escalation
			? "2" + terrain.ToString()[..1].ToLower()
			: terrain.ToString()[..1];
		Escalation = escalation;
	}

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
		await gs.PreRavaging?.InvokeAsync( new RavagingEventArgs( gs ) { Spaces = ravageSpaces } );

		// find ravage spaces that have invaders
		InvaderBinding[] ravageGroups = ravageSpaces
			.Select( gs.Invaders.On )
			.Where( group => group.Tokens.HasInvaders() )
			.Cast<InvaderBinding>()
			.ToArray();

		foreach(InvaderBinding invaderBinding in ravageGroups) {
			var @event = await new RavageAction( gs, invaderBinding ).Exec();
			gs.Log( new InvaderActionEntry( @event.ToString() ) );
		}

	}

	public async Task Build( GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Building:" + Text ) );

		var buildSpaces = gameState.Island.AllSpaces.Where( Matches )
			.ToDictionary( s => s, grp => 1 )
			.ToCountDict();

		BuildEngine buildEngine = gameState.GetBuildEngine();

		// Modify
		var args = new BuildingEventArgs( gameState, new Dictionary<Space, BuildingEventArgs.BuildType>() ) {
			SpaceCounts = buildSpaces,
		};
		await gameState.PreBuilding.InvokeAsync( args );

		// Do build in each space
		var buildGroups = args.SpaceCounts.Keys
			.OrderBy( x => x.Label )
			.Select( x => gameState.Tokens[x] )
			//			.Where( x => x.HasInvaders() ) // We want to log these too
			.ToArray();

		foreach(TokenCountDictionary tokens in buildGroups) {
			int count = args.SpaceCounts[tokens.Space];
			while(count-- > 0) {
				string buildResult = await buildEngine.Exec( args, tokens, gameState );
				gameState.Log( new InvaderActionEntry( tokens.Space.Label + ": gets " + buildResult ) );
			}
		}

	}

	public async Task Explore( GameState gs ) {
		InvaderCard card = this;

		gs.Log( new InvaderActionEntry( "Exploring:" + card.Text ) );

		// Modify
		bool IsExplorerSource( Space space ) { return space.IsOcean || gs.Tokens[space].HasAny( Invader.Town, Invader.City ); }
		var args = new ExploreEventArgs( gs,
			gs.Island.AllSpaces.Where( IsExplorerSource ),
			gs.Island.AllSpaces.Where( card.Matches )
		);
		await gs.PreExplore.InvokeAsync( args );

		var tokenSpacesToExplore = args.WillExplore( gs )
			.Select( x => gs.Tokens[x] )
			.ToArray();

		// Explore
		foreach(var exploreTokens in tokenSpacesToExplore)
			await ExploreSingleSpace( exploreTokens, gs );
	}

	static async Task ExploreSingleSpace( TokenCountDictionary tokens, GameState gs ) {
		// only gets called when explorer is actually going to explore
		var wilds = tokens.Wilds;
		if(wilds == 0) {
			gs.Log( new InvaderActionEntry( tokens.Space + ":gains explorer" ) );
			await tokens.Add( Invader.Explorer.Default, 1, AddReason.Explore );
		} else
			await wilds.Remove( 1, RemoveReason.UsedUp );
	}

}
