namespace SpiritIsland;

/// <summary>
/// The Standard Space - represents 1 visible marked space.
/// </summary>
public class SingleSpaceSpec( Terrain terrain, string label, string startingItems = "" ) : SpaceSpec(label) {

	public Board Board {
		get { return _board; }
		set {
			if(_board != null) 
				throw new InvalidOperationException( "cannot set board twice" );
			_board = value;
			Boards = [ value ];
		}
	}

	public override int InvaderActionCount => _board.InvaderActionCount;

	public override SpaceLayout Layout => _board.Layout.ForSpaceSpec(this);

	public StartUpCounts StartUpCounts { get; } = new StartUpCounts(startingItems);

	public Terrain NativeTerrain { get; set; } = terrain;

	public override bool IsOneOf(params Terrain[] options) => options.Contains(NativeTerrain);

	public override bool Is( Terrain terrain ) => NativeTerrain == terrain;

	public void InitTokens( Space space ) {
		// ! Using 'Adjust' so they don't sqush stuff setup by Adversaries
		StartUpCounts initialCounts = StartUpCounts;
		space.Setup( Human.City, initialCounts.Cities );
		space.Setup( Human.Town, initialCounts.Towns );
		space.Setup( Human.Explorer, initialCounts.Explorers );
		space.Setup( Human.Dahan, initialCounts.Dahan );
		space.Blight.Adjust( initialCounts.Blight ); // don't use AddBlight because that pulls it from the card and triggers blighted island
	}

	Board _board;

}