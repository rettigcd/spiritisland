namespace SpiritIsland.Select;

/// <summary>
/// Groups TokenFrom1Space that applies only to invaders
/// </summary>
public static class Invader {

	public static TokenFromManySpaces ToReplace( string actionVerb, Space space, IEnumerable<ISpaceEntity> options, Present present = Present.Always ) 
		=> new TokenFromManySpaces( $"Select invader to {actionVerb}", space, options.Cast<HumanToken>(), present );

	public static TokenFromManySpaces ToRemove( Space space, IEnumerable<IToken> options ) 
		=> new TokenFromManySpaces( "Remove invader", space, options, Present.Always );

	public static TokenFromManySpaces ToRemoveByHealth( Space space, IEnumerable<HumanToken> invaders, int remainingDamage )
		=> new TokenFromManySpaces( $"Remove up to {remainingDamage} health of invaders.", space, invaders.Where( x => x.RemainingHealth <= remainingDamage ), Present.Done ) ;

	public static TokenFromManySpaces ForIndividualDamage(int damagePerInvader, Space space, IEnumerable<IToken> invaders)
		=> new TokenFromManySpaces( $"Select invader to apply {damagePerInvader} damage", space, invaders.Distinct(), Present.Done );

	public static TokenFromManySpaces ForAggregateDamage( Space space, IToken[] invaderTokens, int aggregateDamage, Present present) 
		=> new TokenFromManySpaces( $"Damage ({aggregateDamage} remaining)",space, invaderTokens, present );

	public static TokenFromManySpaces ForAggregateDamageFromSource( Space space, HumanToken source, IToken[] invaderTokens, int aggregateDamage, Present present )
		=> new TokenFromManySpaces( $"Damage from {source} ({aggregateDamage} remaining)", space, invaderTokens, present );

	public static TokenFromManySpaces ForBadlandDamage(int remainingDamage, Space space, IEnumerable<IToken> invaders)
		=> new TokenFromManySpaces( $"Select invader to apply badland damage ({remainingDamage} remaining)", space, invaders, Present.Done );

	public static TokenFromManySpaces ForStrife( SpaceState tokens, params IEntityClass[] groups )
		=> new TokenFromManySpaces( "Add Strife", 
			tokens.Space,
			(groups!=null && groups.Length>0) ? tokens.OfAnyClass(groups).Cast<IToken>() : tokens.InvaderTokens(), 
			Present.Always 
		);

}