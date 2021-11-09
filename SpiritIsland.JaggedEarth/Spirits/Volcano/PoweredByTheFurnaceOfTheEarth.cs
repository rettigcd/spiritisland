
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	[InnatePower("Powered by the Furnace of the Earth"), Slow, FromPresence(0)]
	public class PoweredByTheFurnaceOfTheEarth {

		[InnateOption("3 earth","Add 1 of your destroyed presence.",0)]
		static public Task Option1(TargetSpaceCtx _ ) {
			return Task.CompletedTask;
		}

		[InnateOption("3 fire","-2 cost.  Gain a Power Card",1)]
		static public Task Option2(TargetSpaceCtx _ ) {
			return Task.CompletedTask;
		}

		[InnateOption("4 earth,4 fire","Move up to 2 of your presence from other lands to target land.",2)]
		static public Task Option3(TargetSpaceCtx _ ) {
			return Task.CompletedTask;
		}

		[InnateOption("5 fire","Return up to 2 of your destroyed presence to your presence tracks.",3)]
		static public Task Option4(TargetSpaceCtx _ ) {
			return Task.CompletedTask;
		}

	}


}
