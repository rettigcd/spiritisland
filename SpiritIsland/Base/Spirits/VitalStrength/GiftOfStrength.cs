using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.VitalStrength {

	[InnatePower("Gift of Strength",Speed.Fast)]
	[TargetSpirit]
	class GiftOfStrength {

		[InnateOption]
#pragma warning disable IDE0060 // Remove unused parameter
		static public Task ActAsync(ActionEngine engine, Spirit target){
#pragma warning restore IDE0060 // Remove unused parameter
			return null;
		}

	}

}
