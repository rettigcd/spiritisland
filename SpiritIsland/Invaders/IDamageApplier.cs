namespace SpiritIsland {
	// Allows to intercept applying specific damage (Flame's Fury)
	public interface IDamageApplier {
		Token ApplyDamage( TokenCountDictionary tokens, int availableDamage, Token invaderToken );
	}


}
