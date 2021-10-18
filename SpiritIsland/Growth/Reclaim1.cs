using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Reclaim1 : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var self = ctx.Self;
			if(self.DiscardPile.Count == 0) return;

			var dict = self.DiscardPile.ToDictionary(c=>$"{c.Text} ${c.Cost} ({c.Speed})",c=>(PowerCard)c);
			var txt = await self.SelectText( "Select card to reclaim.", dict.Keys.ToArray(),Present.Always );
			if(txt != null) {
				PowerCard card = dict[txt];
				if(self.DiscardPile.Contains( card )) {
					self.DiscardPile.Remove( card );
					self.Hand.Add( card );
				}
			}
		}

		public override string Name => "Reclaim(1)";

	}

}
