using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class Reclaim1 : GrowthActionFactory {

		public override async Task Activate( ActionEngine engine ) {
			var discardCards = engine.Self.DiscardPile.ToArray();
			if(discardCards.Length > 0) {
				var dict = discardCards.ToDictionary(c=>$"{c.Text} ${c.Cost} ({c.Speed})",c=>(PowerCard)c);
				var txt = await engine.SelectText( "Select card to reclaim.", dict.Keys.ToArray() );
				if(txt != null) {
					PowerCard card = dict[txt];
					if(engine.Self.DiscardPile.Contains( card )) {
						engine.Self.DiscardPile.Remove( card );
						engine.Self.Hand.Add( card );
					}
				}
			}
		}

		public override string ShortDescription => "Reclaim(1)";

	}

}
