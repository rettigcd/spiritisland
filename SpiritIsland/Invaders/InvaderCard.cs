namespace SpiritIsland;

public class InvaderCard : IOption, IInvaderCard {

	#region static Factories

	public static InvaderCard Stage1( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 1 );
	public static InvaderCard Stage2( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 2 );
	public static InvaderCard Stage2Costal() => new InvaderCard( new CostalFilter(), 2 );
	public static InvaderCard Stage3(Terrain t1,Terrain t2) => new InvaderCard( new DoubleTerrainFilter( t1, t2 ), 3);

	#endregion

	public bool Flipped { get; set; } // setting public so we can Rewind

	public virtual void Flip() { Flipped = true; }

	public bool HoldBack { get; set; } = false;
	public bool Skip { get; set; } = false;

	public int InvaderStage { get; }

	public string Text { get; }
	public bool HasEscalation { get; set; }

	#region Constructors

	public InvaderCard( SpaceFilter filter, int invaderStage ) {
		_filter = filter;
		InvaderStage = invaderStage;
		(HasEscalation, Text) = Init();
	}

	protected InvaderCard( InvaderCard orig ) {
		_filter = orig._filter;
		InvaderStage = orig.InvaderStage;
		(HasEscalation, Text) = Init();
	}

	(bool,string) Init() {
		bool hasEscalation = InvaderStage == 2 && _filter.Text != "Costal";
		string text = (hasEscalation ? "2" : "") + _filter.Text;
		return (hasEscalation,text);
	}

	#endregion

	#region Matching Terrain

	public bool MatchesCard( Space space ) => _filter.Matches( space ); // used only in tests

	public virtual bool MatchesCard( SpaceState space ) => _filter.Matches( space.Space );

	#endregion

	#region Ravage Stuff

	protected virtual bool MatchesCardForRavage( SpaceState spaceState ) => MatchesCard( spaceState );

	public virtual async Task Ravage( GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Ravaging:" + Text ) );
		var ravageSpacesMatchingCard = gameState.AllActiveSpaces.Where( MatchesCardForRavage ).ToList();

		// find ravage spaces that have invaders
		var ravageSpacesWithInvaders = ravageSpacesMatchingCard
			.Where( tokens => tokens.HasInvaders() )
			.ToArray();

		// Add Ravage tokens to spaces with invaders
		foreach(var s in ravageSpacesWithInvaders)
			s.Adjust( TokenType.DoRavage, 1 );

		// get spaces with just-added Ravages + any previously added ravages
		var spacesWithDoRavage = gameState.AllActiveSpaces
			.Where( ss=>ss[TokenType.DoRavage] > 0)
			.ToArray();

		foreach(var ravageSpace in spacesWithDoRavage)
			await DoAllRavagesOn1Space( gameState, ravageSpace );
	}

	static async Task DoAllRavagesOn1Space( GameState gameState, SpaceState ravageSpace ) {
		int ravageCount = PullRavageTokens( ravageSpace );

		while(0 < ravageCount--)
			await new RavageAction( gameState, ravageSpace ).Exec();
	}

	static int PullRavageTokens( SpaceState ravageSpace ) {
		int ravageCount = ravageSpace[TokenType.DoRavage];
		ravageSpace.Init( TokenType.DoRavage, 0 );
		return ravageCount;
	}

	#endregion Ravage Stuff

	#region private fields
	readonly SpaceFilter _filter;
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
