using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.VitalStrength {

	class GiftOfStrength {

		[InnatePower("bob",Speed.Fast)]
		[InnateOption]
		[TargetSpirit]
		static public Task ActAsync(ActionEngine engine, Spirit target){
			return null;
		}

	}

}
