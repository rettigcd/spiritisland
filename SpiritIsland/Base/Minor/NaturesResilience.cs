using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[MinorCard(NaturesResilience.Name,1,Speed.Fast,Element.Earth,Element.Plant,Element.Animal)]
	public class NaturesResilience : BaseAction {

		public const string Name = "Nature's Resilience";
		const string DefendKey = "Defend 6";
		const string RemoveBlightKey = "Remove Blight";

		public NaturesResilience(Spirit self,GameState gameState):base(gameState) {
			_ = Act(self);
		}

		async Task Act(Spirit self){
			// if 2 water, you may INSTEAD remove 1 blight

			var target = await engine.SelectSpace("",self.SacredSites.Range(1));

			bool removeBlight = self.Elements(Element.Water)>=2
				&& await engine.SelectText("Select option",DefendKey, RemoveBlightKey) == RemoveBlightKey;

			if(removeBlight)
				gameState.RemoveBlight(target);
			else 
				gameState.Defend(target,6);

		}

	}
}
