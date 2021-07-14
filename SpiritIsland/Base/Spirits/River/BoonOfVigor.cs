using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class BoonOfVigor {

		public const string Name = "Boon of Vigor";
		[SpiritCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
		static public async Task ActionAsync(ActionEngine engine){
			var spirit = await engine.Api.TargetSpirit();
			spirit.Energy += (spirit==engine.Self) ? 1 : spirit.PurchasedCards.Count;
		}

	}

}