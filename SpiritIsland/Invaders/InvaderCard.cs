namespace SpiritIsland;

public sealed class InvaderCard : IOption {

	#region static Factories

	public static InvaderCard Stage1( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 1 );
	public static InvaderCard Stage2( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 2 );
	public static InvaderCard Stage2Costal() => new InvaderCard( new CostalFilter(), 2 );
	public static InvaderCard Stage3(Terrain t1,Terrain t2) => new InvaderCard( new DoubleTerrainFilter( t1, t2 ), 3);

	#endregion

	public bool Flipped { get; set; } // setting public so we can Rewind

	public async Task Flip( GameState gameState ) { 
		Flipped = true;
		if( CardFlipped != null )
			await CardFlipped.Invoke( gameState ); // await CardFlipped.InvokeInSeries( gameState );
	}

	public event Func<GameState,Task> CardFlipped;

	public bool HoldBack { get; set; } = false;
	public bool Skip { get; set; } = false;

	public int InvaderStage { get; }

	public string Text { get; }
	public bool HasEscalation { get; set; }

	#region Constructors

	public InvaderCard( SpaceFilter filter, int invaderStage ) {
		_filter = filter;
		InvaderStage = invaderStage;
		HasEscalation = InvaderStage == 2 && _filter.Text != "Costal";
		Text = (HasEscalation ? "2" : "") + _filter.Text;

	}

	#endregion

	#region Matching Terrain

	public bool MatchesCard( Space space ) => _filter.Matches( space ); // used only in tests

	public bool MatchesCard( SpaceState space ) => _filter.Matches( space.Space );

	#endregion

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
