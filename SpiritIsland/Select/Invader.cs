namespace SpiritIsland.Select;

/// <summary>
/// Groups TokenFrom1Space that applies only to invaders
/// </summary>
public static class Invader {

	public static ASpaceToken ToReplace( string actionVerb, Space space, IEnumerable<ISpaceEntity> options, Present present = Present.Always ) 
		=> new ASpaceToken( $"Select invader to {actionVerb}", space, options.Cast<HumanToken>(), present );

	public static ASpaceToken ToRemove( Space space, IEnumerable<IToken> options ) 
		=> new ASpaceToken( "Remove invader", space, options, Present.Always );

	public static ASpaceToken ToRemoveByHealth( Space space, IEnumerable<HumanToken> invaders, int remainingDamage )
		=> new ASpaceToken( $"Remove up to {remainingDamage} health of invaders.", space, invaders.Where( x => x.RemainingHealth <= remainingDamage ), Present.Done ) ;

	public static ASpaceToken ForIndividualDamage(int damagePerInvader, Space space, IEnumerable<IToken> invaders)
		=> new ASpaceToken( $"Select invader to apply {damagePerInvader} damage", space, invaders.Distinct(), Present.Done );

	public static ASpaceToken ForAggregateDamage( Space space, IToken[] invaderTokens, int aggregateDamage, Present present) 
		=> new ASpaceToken( $"Damage ({aggregateDamage} remaining)",space, invaderTokens, present );

	public static ASpaceToken ForAggregateDamageFromSource( Space space, HumanToken source, IToken[] invaderTokens, int aggregateDamage, Present present )
		=> new ASpaceToken( $"Damage from {source} ({aggregateDamage} remaining)", space, invaderTokens, present );

	public static ASpaceToken ForBadlandDamage(int remainingDamage, Space space, IEnumerable<IToken> invaders)
		=> new ASpaceToken( $"Select invader to apply badland damage ({remainingDamage} remaining)", space, invaders, Present.Done );

	public static ASpaceToken ForStrife( SpaceState tokens, params IEntityClass[] groups )
		=> new ASpaceToken( "Add Strife", 
			tokens.Space,
			(groups!=null && groups.Length>0) ? tokens.OfAnyClass(groups).Cast<IToken>() : tokens.InvaderTokens(), 
			Present.Always 
		);

}