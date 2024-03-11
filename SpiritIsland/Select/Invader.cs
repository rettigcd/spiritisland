namespace SpiritIsland.An;
using Orig_Space = SpiritIsland.Space;
using IEnumerable_Space = IEnumerable<SpiritIsland.SpaceToken>;

/// <summary>
/// Groups TokenFrom1Space that applies only to invaders
/// </summary>
public static class Invader {

	public static A.SpaceToken ToReplace( string actionVerb, IEnumerable_Space options, Present present = Present.Always ) 
		=> new A.SpaceToken( $"Select invader to {actionVerb}", options, present );

	public static A.SpaceToken ToRemove( IEnumerable_Space options ) 
		=> new A.SpaceToken( "Remove invader", options, Present.Always );

	public static A.SpaceToken ToRemoveByHealth( IEnumerable_Space invaders, int remainingDamage )
		=> new A.SpaceToken( $"Remove up to {remainingDamage} health of invaders.", invaders.Where( x => ((HumanToken)x.Token).RemainingHealth <= remainingDamage ), Present.Done ) ;

	public static A.SpaceToken ForIndividualDamage(int damagePerInvader, IEnumerable_Space invaders )
		=> new A.SpaceToken( $"Select invader to apply {damagePerInvader} damage", invaders.Distinct(), Present.Done );

	public static A.SpaceToken ForAggregateDamage( IEnumerable_Space invaderTokens, int aggregateDamage, Present present) 
		=> new A.SpaceToken( $"Damage ({aggregateDamage} remaining)", invaderTokens, present );

	public static A.SpaceToken ForAggregateDamageFromSource( Orig_Space space, HumanToken source, IToken[] invaderTokens, int aggregateDamage, Present present )
		=> new A.SpaceToken( $"Damage from {source} ({aggregateDamage} remaining)", invaderTokens.OnScopeTokens1(space), present );

	public static A.SpaceToken ForBadlandDamage(int remainingDamage, IEnumerable_Space invaders )
		=> new A.SpaceToken( $"Select invader to apply badland damage ({remainingDamage} remaining)", invaders, Present.Done );

	public static A.SpaceToken ForStrife( SpaceState tokens, params ITokenClass[] groups )
		=> new A.SpaceToken( "Add Strife", 
			((groups!=null && 0<groups.Length) ? (IEnumerable<IToken>)tokens.OfAnyTag(groups) : tokens.InvaderTokens()).OnScopeTokens1(tokens.Space),
			Present.Always 
		);

}