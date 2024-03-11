namespace SpiritIsland;

public sealed class InvaderCard : IOption {

	#region static Factories

	public static InvaderCard Stage1( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 1 );
	public static InvaderCard Stage2( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 2 );
	public static InvaderCard Stage2Costal()       => new InvaderCard( new CoastalFilter(), 2, false );
	public static InvaderCard Stage3(Terrain t1,Terrain t2) => new InvaderCard( new DoubleTerrainFilter( t1, t2 ), 3);

	#endregion

	public bool Flipped { get; set; } // setting public so we can Rewind

	public void Flip() { 
		if ( Flipped ) return;
		Flipped = true;
	}

	public async Task OnReveal() {
		if(CardRevealed is not null)
			await CardRevealed( GameState.Current );
	}

	public event Func<GameState,Task> CardRevealed;

	public int InvaderStage { get; }

	string IOption.Text => Code;
	public readonly string Code;
	public bool TriggersEscalation { get; }

	#region Constructors

	public InvaderCard( InvaderCardSpaceFilter filter, int invaderStage, bool level2TriggersEscalation = true ) {
		Filter = filter;
		InvaderStage = invaderStage;
		TriggersEscalation = InvaderStage == 2 && level2TriggersEscalation;
		Code = (TriggersEscalation ? "2" : "") + Filter.Text;
	}

	#endregion

	#region Matching Terrain

	public bool MatchesCard( SpaceSpec space ) => Filter.Matches( space.ScopeSpace ); // used only in tests

	public bool MatchesCard( Space space ) => Filter.Matches( space );

	#endregion

	#region private fields
	readonly public InvaderCardSpaceFilter Filter; // public so Drawer can draw it.
	#endregion
}

public class SingleTerrainFilter( Terrain terrain ) : InvaderCardSpaceFilter {
	public bool Matches( Space space ) => space.SpaceSpec.Is( Terrain );
	public string Text { get; } = terrain.ToString()[..1];
	readonly public Terrain Terrain = terrain; // public so UI can detect what to draw
}

public class DoubleTerrainFilter( Terrain t1, Terrain t2 ) : InvaderCardSpaceFilter {
	public bool Matches( Space space ) => space.SpaceSpec.IsOneOf( Terrain1, Terrain2 );
	public string Text { get; } = t1.ToString()[..1] + "+" + t2.ToString()[..1];
	readonly public Terrain Terrain1 = t1, Terrain2 = t2;
}


public class CoastalFilter : InvaderCardSpaceFilter {
	public const string Name = "Coastal";
	public string Text => Name;
	public bool Matches( Space space ) => space.SpaceSpec.IsCoastal;
}