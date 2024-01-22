namespace SpiritIsland;

/// <summary>
/// The Standard Space - represents 1 visible marked space.
/// </summary>
public class Space1 : Space {

	#region constructor

	public Space1(Terrain terrain, string label, string startingItems="" )
		:base(label)
	{
		NativeTerrain = terrain;
		StartUpCounts = new StartUpCounts(startingItems);
	}

	#endregion

	public Board Board {
		get { return _board; }
		set {
			if(_board != null) 
				throw new InvalidOperationException( "cannot set board twice" );
			_board = value;
			Boards = [ value ];
		}
	}
	Board _board;
	public override int InvaderActionCount => _board.InvaderActionCount;

	public override bool IsOneOf(params Terrain[] options) => options.Contains(NativeTerrain);

	public override bool Is( Terrain terrain ) => NativeTerrain == terrain;

	public StartUpCounts StartUpCounts { get; }

	public void InitTokens( SpaceState tokens ) {
		// ! Using 'Adjust' so they don't sqush stuff setup by Adversaries
		StartUpCounts initialCounts = StartUpCounts;
		tokens.Setup( Human.City, initialCounts.Cities );
		tokens.Setup( Human.Town, initialCounts.Towns );
		tokens.Setup( Human.Explorer, initialCounts.Explorers );
		tokens.Setup( Human.Dahan, initialCounts.Dahan );
		tokens.Blight.Adjust( initialCounts.Blight ); // don't use AddBlight because that pulls it from the card and triggers blighted island
	}

	public Terrain NativeTerrain {get; set; }

}