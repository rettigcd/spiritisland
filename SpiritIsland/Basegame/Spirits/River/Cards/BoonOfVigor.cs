using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class BoonOfVigor {

		public const string Name = "Boon of Vigor";
		[SpiritCard(BoonOfVigor.Name, 0, Speed.Fast,Element.Sun,Element.Water,Element.Plant)]
		[TargetSpirit]
		static public Task ActionAsync( TargetSpiritCtx ctx){
			ctx.Target.Energy += (ctx.Target==ctx.Self) 
				? 1 
				: ctx.Target.PurchasedCards.Count;
			return Task.CompletedTask;
		}

	}

}