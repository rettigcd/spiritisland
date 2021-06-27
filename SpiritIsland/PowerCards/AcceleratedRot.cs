using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.PowerCards {

	[PowerCard("Accelerated Rot",4,Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
	public class AcceleratedRot : BaseAction {
		public AcceleratedRot(Spirit _,GameState gs):base(gs){
			// range 2, Jungle or Wetland

			// 2 fear
			// 4 damage

			// if you have 3 sun, 2 water, 3 plant
			//	+5 damage, remove 1 blight
		}
	}

}
