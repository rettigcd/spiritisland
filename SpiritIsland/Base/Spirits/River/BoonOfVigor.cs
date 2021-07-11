using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

//	[SpiritCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
	public class BoonOfVigor {

		public const string Name = "Boon of Vigor";
		[SpiritCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
		static public async Task ActionAsync(ActionEngine engine, Spirit self,GameState gameState){
			var spirit = await engine.SelectSpirit(gameState.Spirits);
			spirit.Energy += (spirit==self) ? 1 : spirit.PurchasedCards.Count;
		}

	}

}