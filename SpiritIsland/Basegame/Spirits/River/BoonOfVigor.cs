using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class BoonOfVigor {

		public const string Name = "Boon of Vigor";
		[SpiritCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
		[TargetSpirit]
		static public Task ActionAsync(ActionEngine engine,Spirit target){
			target.Energy += (target==engine.Self) ? 1 : target.PurchasedCards.Count;
			return Task.CompletedTask;
		}

	}

}