namespace SpiritIsland.JaggedEarth;


/// <summary> 
/// Overrides Badlands behavior to include blight
/// </summary>
class WreakVengeanceForTheLandsCorruption : TokenBinding {

	public static SpecialRule Rule => new SpecialRule(
		"Wreak Vengeance for the Land's Corruption",
		"Your actions treat blight on the island as also being badlands"
	);

	readonly TokenBinding _blight;

	public WreakVengeanceForTheLandsCorruption(SpaceState tokens ) 
		: base( new TokenBinding( tokens, Token.Badlands ) )
	{
		_blight = new TokenBinding( new TokenBinding( tokens,Token.Blight ) );
	}

	// Don't need to override Add since base class behavior is correct

	public override int Count => base.Count + _blight.Count;

	public override Task Remove( int count, RemoveReason reason ) {
		throw new InvalidOperationException("No card/action removes badlands."); // Transform to a Murderous Darkness does move badlands, but not using this method.
	}

}