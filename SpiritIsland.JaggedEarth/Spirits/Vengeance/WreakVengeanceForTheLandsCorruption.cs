namespace SpiritIsland.JaggedEarth {

	/// <summary> Overrides Badlands behavior </summary>
	class WreakVengeanceForTheLandsCorruption : TokenBinding {

		readonly TokenBinding blight;

		public WreakVengeanceForTheLandsCorruption(TokenCountDictionary tokens ) : base( tokens, TokenType.Badlands ) {
			blight = new TokenBinding(tokens,TokenType.Blight);
		}

		public static SpecialRule Rule => new SpecialRule(
			"Wreak Vengeance for the Land's Corruption",
			"Your actions treat blight on the island as also being badlands"
		);

		// Don't need to override Add since base class behavior is correct

		public override int Count => base.Count + blight.Count;

		public override void Remove( int count ) {
			int realBadlands = base.Count;
			int realBandlandsToRemove = System.Math.Min(realBadlands,count);
			base.Remove( realBandlandsToRemove );
			int blightToRemove = count - realBandlandsToRemove;
			blight.Remove( blightToRemove ); // !! doesn't go back to the card - should it?
		}

		public override void Destroy( int count ) {
			int realBadlands = base.Count;
			int realBandlandsToDestroy = System.Math.Min(realBadlands,count);
			base.Destroy( realBandlandsToDestroy );
			int blightToDestory = count - realBandlandsToDestroy;
			blight.Destroy( blightToDestory ); // !! doesn't go back to the card - should it?
		}

	}

}
