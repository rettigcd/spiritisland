#nullable enable
namespace SpiritIsland.An;
using Orig_SpaceSpec = SpiritIsland.SpaceSpec;
using IEnumerable_SpaceToken = IEnumerable<SpiritIsland.SpaceToken>;

/// <summary>
/// Groups TokenFrom1Space that applies only to invaders
/// </summary>
public static class Invader {

	public static A.SpaceTokenDecision ToReplace( string actionVerb, IEnumerable_SpaceToken options, Present present = Present.Always ) 
		=> new A.SpaceTokenDecision( $"Select invader to {actionVerb}", options, present );

	public static A.SpaceTokenDecision ToRemove( IEnumerable_SpaceToken options ) 
		=> new A.SpaceTokenDecision( "Remove invader", options, Present.Always );

	public static A.SpaceTokenDecision ToRemoveByHealth( IEnumerable_SpaceToken invaders, int remainingDamage )
		=> new A.SpaceTokenDecision( $"Remove up to {remainingDamage} health of invaders.", invaders.Where( x => ((HumanToken)x.Token).RemainingHealth <= remainingDamage ), Present.Done ) ;

	public static A.SpaceTokenDecision ForIndividualDamage(int damagePerInvader, IEnumerable_SpaceToken invaders )
		=> new A.SpaceTokenDecision( $"Select invader to apply {damagePerInvader} damage", invaders.Distinct(), Present.Done );

	public static A.SpaceTokenDecision ForAggregateDamage( IEnumerable_SpaceToken invaderSpaces, int aggregateDamage, Present present) 
		=> new A.SpaceTokenDecision( $"Damage ({aggregateDamage} remaining)", invaderSpaces, present );

	public static A.SpaceTokenDecision ForAggregateDamageFromSource( Orig_SpaceSpec space, HumanToken source, IToken[] invaderTokens, int aggregateDamage, Present present )
		=> new A.SpaceTokenDecision( $"Damage from {source} ({aggregateDamage} remaining)", invaderTokens.On(space.ScopeSpace), present );

	public static A.SpaceTokenDecision ForBadlandDamage(int remainingDamage, IEnumerable_SpaceToken invaders )
		=> new A.SpaceTokenDecision( $"Select invader to apply badland damage ({remainingDamage} remaining)", invaders, Present.Done );

	public static A.SpaceTokenDecision ForStrife( Space space, params ITokenClass[] groups )
		=> new A.SpaceTokenDecision( "Add Strife", 
			((groups!=null && 0<groups.Length) ? (IEnumerable<IToken>)space.OfAnyTag(groups) : space.InvaderTokens()).On(space),
			Present.Always 
		);

}