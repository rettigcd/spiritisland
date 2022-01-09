using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary>
	/// A card in spirit's Hand, may be Played (triggering its elements to be added)
	/// </summary>
	public class PlayCardForCost : IActionFactory {

		public bool CouldActivateDuring( Phase speed, Spirit _ ) 
			=> speed == Phase.Fast || speed == Phase.Slow;

		public string Name => "Play Card for Cost";

		public string Text => Name;

		public async Task ActivateAsync( SelfCtx ctx ) {

			int maxCardCost = ctx.Self.Energy;
			var options = ctx.Self.Hand.OfType<PowerCard>()
				.Where(card=>card.Cost<=maxCardCost)
				.ToArray();
			if(options.Length == 0) return;

			PowerCard powerCard = await ctx.Self.SelectPowerCard( "Select card to play", options.Where( x => x.Cost <= maxCardCost ), CardUse.Play, Present.Always );
			if(powerCard != null)
				ctx.Self.PlayCard( powerCard );
		}

	}


}
