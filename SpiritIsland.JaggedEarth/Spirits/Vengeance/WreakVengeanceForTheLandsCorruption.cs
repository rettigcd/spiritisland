namespace SpiritIsland.JaggedEarth;

/// <summary> Overrides Badlands behavior </summary>
class WreakVengeanceForTheLandsCorruption : TokenBinding {

	readonly TokenBinding _blight;

	public WreakVengeanceForTheLandsCorruption(SpaceState tokens, UnitOfWork actionScope ) 
		: base( new TokenBindingNoEvents( tokens, Token.Badlands ), actionScope )
	{
		_blight = new TokenBinding( new TokenBindingNoEvents( tokens,Token.Blight ), actionScope );
	}

	public static SpecialRule Rule => new SpecialRule(
		"Wreak Vengeance for the Land's Corruption",
		"Your actions treat blight on the island as also being badlands"
	);

	// Don't need to override Add since base class behavior is correct

	public override int Count => base.Count + _blight.Count;

	public override async Task Remove( int count, RemoveReason reason ) {
		// Remove real Badlands first
		int realBadlands = base.Count;
		int realBandlandsToRemove = System.Math.Min(realBadlands,count);
		await base.Remove( realBandlandsToRemove, reason );

		// if any left over, remove the blight instead
		int blightToRemove = count - realBandlandsToRemove;
		await _blight.Remove( blightToRemove, reason );
		// !! doesn't go back to the card - should it?
		// !!! also not sure what happens if they try to move a badland
	}

	// !!! review this whole class to see if it is generating property Token-api events - particularly around blight

}