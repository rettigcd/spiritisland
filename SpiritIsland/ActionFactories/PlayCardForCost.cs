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

		public async Task ActivateAsync( SpiritGameStateCtx ctx ) {

			int maxCardCost = ctx.Self.Energy;
			var options = ctx.Self.UsedActions.OfType<PowerCard>() // can't use Discard pile because those cards are from prior rounds.  // !!! needs tests
				.Where(card=>ctx.Self.IsActiveDuring(ctx.GameState.Phase,card)) 
				.Where(card=>card.Cost<=maxCardCost)
				.ToArray();
			if(options.Length == 0) return;

			PowerCard powerCard = await ctx.Self.SelectPowerCard( "Select card to replay", options.Where( x => x.Cost <= maxCardCost ), CardUse.Repeat, Present.Always );
			if(powerCard != null)
				ctx.Self.PlayCard( powerCard );
		}

	}


}
