namespace SpiritIsland;

public class Space1 : Space {

	#region constructor

	public Space1(Terrain terrain, string label,string startingItems="")
		:base(label)
	{
		this.terrain = terrain;
		this.StartUpCounts = new StartUpCounts(startingItems);
	}

	#endregion

	public override bool IsOneOf(params Terrain[] options) => options.Contains(terrain);

	public override bool Is( Terrain terrain ) => this.terrain == terrain;

	public StartUpCounts StartUpCounts { get; }

	public void InitTokens( TokenCountDictionary tokens ) {
		// ! Using 'Adjust' so they don't sqush stuff setup by Adversaries
		StartUpCounts initialCounts = StartUpCounts;
		tokens.AdjustDefault( Invader.City, initialCounts.Cities );
		tokens.AdjustDefault( Invader.Town, initialCounts.Towns );
		tokens.AdjustDefault( Invader.Explorer, initialCounts.Explorers );
		tokens.AdjustDefault( TokenType.Dahan, initialCounts.Dahan );
		tokens.Blight.Adjust( initialCounts.Blight ); // don't use AddBlight because that pulls it from the card and triggers blighted island
	}

	readonly Terrain terrain;

}