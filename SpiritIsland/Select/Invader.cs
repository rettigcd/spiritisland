namespace SpiritIsland.Select;

/// <summary>
/// Groups TokenFrom1Space that applies only to invaders
/// </summary>
public static class Invader {

	public static TokenFrom1Space ToReplace( string actionVerb, SpiritIsland.Space space, IEnumerable<Token> options, Present present = Present.Always ) 
		=> new TokenFrom1Space( $"Select invader to {actionVerb}", space, options.Cast<HealthToken>(), present );

	public static TokenFrom1Space ToRemove( SpiritIsland.Space space, IEnumerable<Token> options ) 
		=> new TokenFrom1Space( "Remove invader", space, options, Present.Always );

	public static TokenFrom1Space ToRemoveByHealth( SpiritIsland.Space space, IEnumerable<HealthToken> invaders, int remainingDamage )
		=> new TokenFrom1Space( $"Remove up to {remainingDamage} health of invaders.", space, invaders.Where( x => x.RemainingHealth <= remainingDamage ), Present.Done ) ;

	public static TokenFrom1Space ForIndividualDamage(int damagePerInvader, SpiritIsland.Space space, IEnumerable<Token> invaders)
		=> new TokenFrom1Space( $"Select invader to apply {damagePerInvader} damage", space, invaders.Distinct(), Present.Done );

	public static TokenFrom1Space ForAggregateDamage( SpiritIsland.Space space, Token[] invaderTokens, int aggregateDamage, Present present) 
		=> new TokenFrom1Space($"Damage ({aggregateDamage} remaining)",space,invaderTokens, present );

	public static TokenFrom1Space ForAggregateDamageFromSource( SpiritIsland.Space space, HealthToken source, Token[] invaderTokens, int aggregateDamage, Present present )
		=> new TokenFrom1Space( $"Damage from {source} ({aggregateDamage} remaining)", space, invaderTokens, present );

	public static TokenFrom1Space ForBadlandDamage(int remainingDamage, SpiritIsland.Space space, IEnumerable<Token> invaders)
		=> new TokenFrom1Space( $"Select invader to apply badland damage ({remainingDamage} remaining)", space, invaders, Present.Done );

	public static TokenFrom1Space ForStrife( TokenCountDictionary tokens, params TokenClass[] groups )
		=> new TokenFrom1Space( "Add Strife", 
			tokens.Space,
			(groups!=null && groups.Length>0) ? tokens.OfAnyType(groups) : tokens.InvaderTokens(), 
			Present.Always 
		);

}