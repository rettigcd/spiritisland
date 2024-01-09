namespace SpiritIsland;

public sealed class InvaderCard : IOption {

	#region static Factories

	public static InvaderCard Stage1( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 1 );
	public static InvaderCard Stage2( Terrain t1 ) => new InvaderCard( new SingleTerrainFilter(t1), 2 );
	public static InvaderCard Stage2Costal()       => new InvaderCard( new CoastalFilter(), 2 );
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

	public string Text { get; }
	public bool HasEscalation { get; set; }

	#region Constructors

	public InvaderCard( InvaderCardSpaceFilter filter, int invaderStage ) {
		Filter = filter;
		InvaderStage = invaderStage;
		HasEscalation = InvaderStage == 2 && Filter.Text != CoastalFilter.Name;
		Text = (HasEscalation ? "2" : "") + Filter.Text;
	}

	#endregion

	#region Matching Terrain

	public bool MatchesCard( Space space ) => Filter.Matches( space ); // used only in tests

	public bool MatchesCard( SpaceState space ) => Filter.Matches( space.Space );

	#endregion

	#region private fields
	readonly public InvaderCardSpaceFilter Filter; // public so Drawer can draw it.
	#endregion
}

public class SingleTerrainFilter : InvaderCardSpaceFilter {
	public SingleTerrainFilter( Terrain terrain ) {
		this.Terrain = terrain;
		this.Text = terrain.ToString()[..1];
	}
	public bool Matches( Space space ) => space.Is( Terrain );
	public string Text { get; }
	readonly public Terrain Terrain; // public so UI can detect what to draw
}

public class DoubleTerrainFilter : InvaderCardSpaceFilter {
	public DoubleTerrainFilter( Terrain t1, Terrain t2 ) {
		Terrain1 = t1;
		Terrain2 = t2;
		Text = t1.ToString()[..1] + "+" + t2.ToString()[..1];
	}
	public bool Matches( Space space ) => space.IsOneOf( Terrain1, Terrain2 );
	public string Text { get; }
	readonly public Terrain Terrain1, Terrain2;
}


public class CoastalFilter : InvaderCardSpaceFilter {
	public const string Name = "Coastal";
	public string Text => Name;
	public bool Matches( Space space ) => space.IsCoastal;
}