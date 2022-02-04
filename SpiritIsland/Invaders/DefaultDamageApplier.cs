namespace SpiritIsland;

public class DefaultDamageApplier : IDamageApplier {

	Token IDamageApplier.ApplyDamage( TokenCountDictionary tokens, int availableDamage, Token invaderToken ) {
		var damagedInvader = invaderToken.ResultingDamagedInvader( availableDamage );
		tokens.Adjust( invaderToken, -1 );
		if(0 < damagedInvader.Health) // only track living invaders
			tokens.Adjust( damagedInvader, 1 );
		return damagedInvader;
	}

}