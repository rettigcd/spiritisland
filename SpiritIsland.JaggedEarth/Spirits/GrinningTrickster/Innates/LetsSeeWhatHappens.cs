using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Let's See What Happens"), Fast, FromPresence(1,Target.Invaders)]
	public class LetsSeeWhatHappens {

		[InnateOption("1 moon,1 fire,2 air","Discard Minor Powers from the deck until you get one that targets a land. Use immediately. All 'up to' instructions must be used at max and 'OR's treated as 'AND's ")]
		static public Task Option1(TargetSpaceCtx ctx ) {
			return ctx == null ? null : Task.CompletedTask;
		}

		[InnateOption("2 moon,1 fire,2 air","You may Forget a Power Card to gain the just-used Power Card and 1 Energy")]
		static public Task Option2(TargetSpaceCtx ctx ) {
			return ctx == null ? null : Task.CompletedTask;
		}


	}

}
