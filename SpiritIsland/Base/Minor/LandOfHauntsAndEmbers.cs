using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Minor {
	class LandOfHauntsAndEmbers {

		[MinorCard("Land of Haunts and Embers",0,Speed.Fast,Element.Moon,Element.Fire,Element.Air)]
		static public async Task Act(ActionEngine engine){
			// range 2
			// 2 fear, push up to 2 explorers/towns
			// if target has blight, +2 fear, push up to 2 more explorers/towns, add 1 blight

			_ = await engine.TargetSpace_Presence(0); // !!! wrong - replace 
		}

	}
}
