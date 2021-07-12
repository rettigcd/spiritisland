using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {


	[MinorCard(EncompassingWard.Name,1,Speed.Fast,Element.Sun,Element.Water,Element.Earth)]
	public class EncompassingWard : BaseAction {

		public const string Name = "Encompassing Ward";

		public EncompassingWard(Spirit spirit,GameState gs):base(spirit,gs){
			_ = Act();
		}
		async Task Act(){
			var spirit = await engine.TargetSpirit();
			// defend 2 in every land where spirit has presence
			foreach(var space in spirit.Presence.Distinct())
				gameState.Defend(space,2);
		}
	}
}
