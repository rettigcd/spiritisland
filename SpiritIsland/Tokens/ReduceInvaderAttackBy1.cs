namespace SpiritIsland;

public class ReduceInvaderAttackBy1 : BaseModEntity, ISkipRavages, IEndWhenTimePasses {

	readonly HumanTokenClass[] _classesToReduce;
	readonly int _reduce;

	public ReduceInvaderAttackBy1( int reduce, params HumanTokenClass[] classesToReduce ) {
		_reduce = reduce;
		_classesToReduce = classesToReduce;
	}

	public UsageCost Cost => UsageCost.Free;

	public Task<bool> Skip( SpaceState space ) {

		// !!! BUG - any token pushed out during ravage (like an explorer for some adversay) won't get their attack back.

		// Token Records attacks for each Invaders type  (Assumes all invaders have the same attack!!!)
		Dictionary<HumanTokenClass, int> reducedClasses = new Dictionary<HumanTokenClass, int>();

		// Token Reduces Attack of invaders by 1
		foreach(HumanToken orig in space.HumanOfAnyTag( _classesToReduce ).ToArray()) {
			int reduce = Math.Min( _reduce, orig.Attack );
			if(reduce == 0) continue;
			reducedClasses[orig.HumanClass] = reduce;
			AdjustAttack( space, orig, -reduce );
		}

		// At end of Action, invaders are are restored to original attack.
		ActionScope.Current.AtEndOfThisAction( scope => {
			// Restore original attacks
			HumanToken[] endingInvaders = space.HumanOfAnyTag( reducedClasses.Keys.ToArray() ).ToArray();
			foreach(HumanToken ending in endingInvaders)
				AdjustAttack( space, ending, reducedClasses[ending.HumanClass] );
		} );

		return Task.FromResult( false ); // don't skip
	}

	static void AdjustAttack( SpaceState space, HumanToken orig, int adjust ) {
		space.Init( orig.SetAttack( orig.Attack + adjust ), space[orig] );
		space.Init( orig, 0 );
	}
}