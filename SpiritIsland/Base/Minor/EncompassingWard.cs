using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {


	public class EncompassingWard {

		public const string Name = "Encompassing Ward";

		[MinorCard(EncompassingWard.Name,1,Speed.Fast,Element.Sun,Element.Water,Element.Earth)]
		static public async Task Act(ActionEngine engine){
			var spirit = await engine.Api.TargetSpirit();
			// defend 2 in every land where spirit has presence
			foreach(var space in spirit.Presence.Distinct())
				engine.GameState.Defend(space,2);
		}
	}
}
