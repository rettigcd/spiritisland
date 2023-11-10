namespace SpiritIsland.An;
using Orig_Space = SpiritIsland.Space;

/// <summary>
/// Groups TokenFrom1Space that applies only to invaders
/// </summary>
public static class Invader {

	public static A.SpaceToken ToReplace( string actionVerb, Orig_Space space, IEnumerable<ISpaceEntity> options, Present present = Present.Always ) 
		=> new A.SpaceToken( $"Select invader to {actionVerb}", space, options.Cast<HumanToken>(), present );

	public static A.SpaceToken ToRemove( Orig_Space space, IEnumerable<IToken> options ) 
		=> new A.SpaceToken( "Remove invader", space, options, Present.Always );

	public static A.SpaceToken ToRemoveByHealth( Orig_Space space, IEnumerable<HumanToken> invaders, int remainingDamage )
		=> new A.SpaceToken( $"Remove up to {remainingDamage} health of invaders.", space, invaders.Where( x => x.RemainingHealth <= remainingDamage ), Present.Done ) ;

	public static A.SpaceToken ForIndividualDamage(int damagePerInvader, Orig_Space space, IEnumerable<IToken> invaders)
		=> new A.SpaceToken( $"Select invader to apply {damagePerInvader} damage", space, invaders.Distinct(), Present.Done );

	public static A.SpaceToken ForAggregateDamage( Orig_Space space, IToken[] invaderTokens, int aggregateDamage, Present present) 
		=> new A.SpaceToken( $"Damage ({aggregateDamage} remaining)",space, invaderTokens, present );

	public static A.SpaceToken ForAggregateDamageFromSource( Orig_Space space, HumanToken source, IToken[] invaderTokens, int aggregateDamage, Present present )
		=> new A.SpaceToken( $"Damage from {source} ({aggregateDamage} remaining)", space, invaderTokens, present );

	public static A.SpaceToken ForBadlandDamage(int remainingDamage, Orig_Space space, IEnumerable<IToken> invaders)
		=> new A.SpaceToken( $"Select invader to apply badland damage ({remainingDamage} remaining)", space, invaders, Present.Done );

	public static A.SpaceToken ForStrife( SpaceState tokens, params IEntityClass[] groups )
		=> new A.SpaceToken( "Add Strife", 
			tokens.Space,
			(groups!=null && groups.Length>0) ? tokens.OfAnyClass(groups).Cast<IToken>() : tokens.InvaderTokens(), 
			Present.Always 
		);

}