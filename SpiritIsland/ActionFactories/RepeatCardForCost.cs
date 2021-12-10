using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	// ** NOTE **
	// Repeat != Replay
	// Repeat => a card in play may be activated a 2nd time (already in play so no new elements added)
	// Play => a card in Hand may be played (new elements added)

	/// <summary>
	/// A card already in Play, may be Activated an additional time. (no new elements)
	/// </summary>
	public class RepeatCardForCost : IActionFactory {

		public bool CouldActivateDuring( Phase speed, Spirit _ ) 
			=> speed == Phase.Fast || speed == Phase.Slow;

		public string Name => "Replay Card for Cost";

		public string Text => Name;

		public async Task ActivateAsync( SpiritGameStateCtx ctx ) {

			int maxCardCost = ctx.Self.Energy;
			var options = ctx.Self.UsedActions.OfType<PowerCard>() // can't use Discard pile because those cards are from prior rounds.  // !!! needs tests
				.Where(card=>ctx.Self.IsActiveDuring(ctx.GameState.Phase,card)) 
				.Where(card=>card.Cost<=maxCardCost)
				.ToArray();
			if(options.Length == 0) return;

			PowerCard powerCard = await ctx.Self.SelectPowerCard( "Select card to replay", options.Where( x => x.Cost <= maxCardCost ), CardUse.Replay, Present.Always );
			if(powerCard == null) return;

			ctx.Self.Energy -= powerCard.Cost;
			ctx.Self.AddActionFactory( powerCard );

		}

	}


}
