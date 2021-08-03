using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Core {

	public class Reclaim1 : GrowthActionFactory {

		public override async Task Activate( ActionEngine engine ) {
			var options = engine.Self.DiscardPile.ToArray();
			if(options.Length > 0) {
				var card = (PowerCard)await engine.SelectFactory( "Select card to reclaim.", options ); // !! create a SelectPowerCard that returns PowerCard and use it for forgeting and reclaining
				if(card != null && engine.Self.DiscardPile.Contains( card )) {
					engine.Self.DiscardPile.Remove( card );
					engine.Self.Hand.Add( card );
				}
			}
		}

		public override string ShortDescription => "Reclaim(1)";

	}

}
