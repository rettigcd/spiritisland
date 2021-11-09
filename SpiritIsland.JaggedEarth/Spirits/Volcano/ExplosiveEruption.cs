
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {


	[InnatePower("Explosive Eruption"), Fast, FromPresence(0)]
	public class ExplosiveEruption {

		[InnateOption("2 fire, 2 earth","2 destroyedpresence In one land withing range 1, X Damage",0)]
		static public Task Option1(TargetSpaceCtx _ ) {
			return Task.CompletedTask;
		}

		[InnateOption("3 fire, 3 earth","4 destroyedpresence Generate X fear.",1)]
		static public Task Option2(TargetSpaceCtx _ ) {
			return Task.CompletedTask;
		}

		[InnateOption("4 fire, 2 air, 4 earth","6 destroyedpresence In each land within range 1, 4 Damage.  Add 1 blight to target land; doing so does not Destroy your presence.",2)]
		static public Task Option3(TargetSpaceCtx _ ) {
			return Task.CompletedTask;
		}

		[InnateOption("5 fire, 3 air, 5 earth","10 destroyedpresence In each land withing range 2, +4 Damage.  In each land adjacent to the target, add 1 blight if it doesn't have any.",3)]
		static public Task Option4(TargetSpaceCtx _ ) {
			return Task.CompletedTask;
		}

	}


}
