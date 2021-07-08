using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor : BaseAction{

		public const string Name = "Boon of Vigor";

		readonly Spirit self;
		public BoonOfVigor(Spirit self,GameState gameState)
			:base(gameState)
		{
			this.self = self;
			_ = ActionAsync();
		}

		async Task ActionAsync(){
			var spirit = await engine.SelectSpirit(gameState.Spirits);
			spirit.Energy += (spirit==self) ? 1 : spirit.PurchasedCards.Count;
		}

	}

}