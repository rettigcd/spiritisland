using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class BoonOfVigor {

		public const string Name = "Boon of Vigor";
		[SpiritCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
		[TargetSpirit]
		static public Task ActionAsync( TargetSpiritCtx ctx){
			ctx.Other.Energy += (ctx.Other==ctx.Self) 
				? 1 
				: ctx.Other.PurchasedCards.Count;
			return Task.CompletedTask;
		}

	}

}