using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Reclaim1 : GrowthActionFactory {

		public override async Task ActivateAsync( Spirit self, GameState _ ) {
			var discardCards = self.DiscardPile.ToArray();
			if(discardCards.Length > 0) {
				var dict = discardCards.ToDictionary(c=>$"{c.Text} ${c.Cost} ({c.Speed})",c=>(PowerCard)c);
				var txt = await self.SelectText( "Select card to reclaim.", dict.Keys.ToArray() );
				if(txt != null) {
					PowerCard card = dict[txt];
					if(self.DiscardPile.Contains( card )) {
						self.DiscardPile.Remove( card );
						self.Hand.Add( card );
					}
				}
			}
		}

		public override string ShortDescription => "Reclaim(1)";

	}

}
