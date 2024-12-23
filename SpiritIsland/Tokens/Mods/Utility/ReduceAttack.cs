namespace SpiritIsland;

/// <summary>
/// Before Ravage begins, reduces Human attack, then restores it immediately following the Ravage
/// </summary>
/// <param name="reduce"></param>
/// <param name="classesToReduce"></param>
public class ReduceAttack( int reduce, params HumanTokenClass[] classesToReduce ) 
	: BaseModEntity, IConfigRavages, IEndWhenTimePasses
{
	readonly int _reduce = reduce;

	Task IConfigRavages.Config( Space space ) {

		// !!! BUG - any token pushed out during ravage (like an explorer for some adversay) won't get their attack back.

		// Token Records attacks for each Invaders type  (Assumes all invaders have the same attack!!!)
		Dictionary<HumanTokenClass, int> reducedClasses = [];

		// Token Reduces Attack of invaders by 1
		foreach(HumanToken orig in space.HumanOfAnyTag( classesToReduce ).ToArray()) {
			int reduce = Math.Min( _reduce, orig.Attack );
			if(reduce == 0) continue;
			reducedClasses[orig.HumanClass] = reduce;
			AdjustAttack( space, orig, -reduce );
		}

		// At end of Ravage, invaders are are restored to original attack.
		ActionScope.Current.AtEndOfThisAction( scope => {
			// Restore original attacks
			HumanToken[] endingInvaders = [..space.HumanOfAnyTag( reducedClasses.Keys.ToArray() )];
			foreach(HumanToken ending in endingInvaders)
				AdjustAttack( space, ending, reducedClasses[ending.HumanClass] );
		} );

		return Task.CompletedTask;
	}

	static void AdjustAttack( Space space, HumanToken orig, int adjust ) {
		space.Init( orig.SetAttack( Math.Max(0,orig.Attack + adjust) ), space[orig] );
		space.Init( orig, 0 );
	}
}