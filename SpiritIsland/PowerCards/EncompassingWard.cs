using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.PowerCards {


	[PowerCard("Encompasing Ward",1,Speed.Fast)]
	public class EncompassingWard : BaseAction {
		public EncompassingWard(Spirit spirit,GameState gs):base(gs){
			engine.decisions.Push(new TargetSpirit(gs.Spirits,SelectSpirit));
		}
		void SelectSpirit(Spirit spirit){
			// defend 2 in every land where spirit has presence
			foreach(var space in spirit.Presence.Distinct())
				gameState.Defend(space,2);
		}
	}
}
