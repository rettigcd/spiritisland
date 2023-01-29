namespace SpiritIsland.Select;

/// <summary>
/// Groups TokenFrom1Space that applies only to invaders
/// </summary>
public static class Invader {

	public static TokenFromManySpaces ToReplace( string actionVerb, SpiritIsland.Space space, IEnumerable<Token> options, Present present = Present.Always ) 
		=> new TokenFromManySpaces( $"Select invader to {actionVerb}", space, options.Cast<HealthToken>(), present );

	public static TokenFromManySpaces ToRemove( SpiritIsland.Space space, IEnumerable<IVisibleToken> options ) 
		=> new TokenFromManySpaces( "Remove invader", space, options, Present.Always );

	public static TokenFromManySpaces ToRemoveByHealth( SpiritIsland.Space space, IEnumerable<HealthToken> invaders, int remainingDamage )
		=> new TokenFromManySpaces( $"Remove up to {remainingDamage} health of invaders.", space, invaders.Where( x => x.RemainingHealth <= remainingDamage ), Present.Done ) ;

	public static TokenFromManySpaces ForIndividualDamage(int damagePerInvader, SpiritIsland.Space space, IEnumerable<IVisibleToken> invaders)
		=> new TokenFromManySpaces( $"Select invader to apply {damagePerInvader} damage", space, invaders.Distinct(), Present.Done );

	public static TokenFromManySpaces ForAggregateDamage( SpiritIsland.Space space, IVisibleToken[] invaderTokens, int aggregateDamage, Present present) 
		=> new TokenFromManySpaces( $"Damage ({aggregateDamage} remaining)",space, invaderTokens, present );

	public static TokenFromManySpaces ForAggregateDamageFromSource( SpiritIsland.Space space, HealthToken source, IVisibleToken[] invaderTokens, int aggregateDamage, Present present )
		=> new TokenFromManySpaces( $"Damage from {source} ({aggregateDamage} remaining)", space, invaderTokens, present );

	public static TokenFromManySpaces ForBadlandDamage(int remainingDamage, SpiritIsland.Space space, IEnumerable<IVisibleToken> invaders)
		=> new TokenFromManySpaces( $"Select invader to apply badland damage ({remainingDamage} remaining)", space, invaders, Present.Done );

	public static TokenFromManySpaces ForStrife( SpaceState tokens, params TokenClass[] groups )
		=> new TokenFromManySpaces( "Add Strife", 
			tokens.Space,
			(groups!=null && groups.Length>0) ? tokens.OfAnyClass(groups).Cast<IVisibleToken>() : tokens.InvaderTokens(), 
			Present.Always 
		);

}