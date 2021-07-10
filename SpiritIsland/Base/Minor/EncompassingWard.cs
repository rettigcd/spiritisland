using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {


	[MinorCard(EncompassingWard.Name,1,Speed.Fast,Element.Sun,Element.Water,Element.Earth)]
	public class EncompassingWard : BaseAction {

		public const string Name = "Encompassing Ward";

		public EncompassingWard(Spirit _,GameState gs):base(gs){
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			Act();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}
		async Task Act(){
			var spirit = await engine.SelectSpirit();
			// defend 2 in every land where spirit has presence
			foreach(var space in spirit.Presence.Distinct())
				gameState.Defend(space,2);
		}
	}
}
