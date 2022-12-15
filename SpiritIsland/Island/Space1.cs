using System.Drawing;

namespace SpiritIsland;

public class Space1 : Space {

	#region constructor

	public Space1(Terrain terrain, string label, SpaceLayout layout, string startingItems="" )
		:base(label)
	{
		_terrain = terrain;
		StartUpCounts = new StartUpCounts(startingItems);
		Layout = layout;
	}

	#endregion

	public override bool IsOneOf(params Terrain[] options) => options.Contains(_terrain);

	public override bool Is( Terrain terrain ) => _terrain == terrain;

	public override SpaceLayout Layout { get; }

	public StartUpCounts StartUpCounts { get; }

	public void InitTokens( SpaceState tokens ) {
		// ! Using 'Adjust' so they don't sqush stuff setup by Adversaries
		StartUpCounts initialCounts = StartUpCounts;
		tokens.AdjustDefault( Invader.City, initialCounts.Cities );
		tokens.AdjustDefault( Invader.Town, initialCounts.Towns );
		tokens.AdjustDefault( Invader.Explorer, initialCounts.Explorers );
		tokens.AdjustDefault( TokenType.Dahan, initialCounts.Dahan );
		tokens.Blight.Adjust( initialCounts.Blight ); // don't use AddBlight because that pulls it from the card and triggers blighted island
	}

	readonly Terrain _terrain;

}