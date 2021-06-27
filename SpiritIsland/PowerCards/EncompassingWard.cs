using System.Linq;

namespace SpiritIsland.PowerCards {


	[PowerCard("Encompasing Ward",1,Speed.Fast)]
	public class EncompassingWard : TargetSpiritAction {
		public EncompassingWard(Spirit self,GameState gs):base(self,gs){}
		protected override void SelectSpirit(Spirit spirit){
			// defend 2 in every land where spirit has presence
			foreach(var space in spirit.Presence.Distinct())
				gameState.Defend(space,2);
		}
	}
}
