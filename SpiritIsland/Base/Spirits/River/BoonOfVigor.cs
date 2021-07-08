using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor : BaseAction{

		public const string Name = "Boon of Vigor";

		public BoonOfVigor(Spirit riverSpirit,GameState gameState):base(gameState){
			_ = ActionAsync(riverSpirit);
		}

		async Task ActionAsync(Spirit riverSpirit){
			var spirit = await engine.SelectSpirit(gameState.Spirits);
			spirit.Energy += (spirit==riverSpirit) ? 1 : spirit.PurchasedCards.Count;
		}

	}

}