#nullable enable
namespace SpiritIsland;

public record SSS( Terrain terrain, string label, string startingItems = "" );


/// <summary>
/// The Standard Space - represents 1 visible marked space.
/// </summary>
public class SingleSpaceSpec( Terrain terrain, string label, Board board, string startingItems = "" ) : SpaceSpec(label,[board]) {

	public Board Board => _board;

	public override int InvaderActionCount => _board.InvaderActionCount;

	override public SpaceLayout Layout => _spaceLayout ??= _board.Layout.ForSpaceSpec(this);
	public void SetLayout(SpaceLayout layout) { _spaceLayout = layout; }
	SpaceLayout? _spaceLayout;

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

	readonly Board _board = board;

}