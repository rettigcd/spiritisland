using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	/// <summary> Created during fast action.  Could be used during Fast or Slow </summary>
	public class ReplayCardForCost : IActionFactory {

		#region constructors

		/// <summary> No limit on card that maybe replayed </summary>
		public ReplayCardForCost() {
			this.maxCost = int.MaxValue;
		}

		/// <summary> Limits which cards that can be replayed </summary>
		public ReplayCardForCost(int maxCost) {
			this.maxCost = maxCost;
		}

		#endregion

		public bool CouldActivateDuring( Phase speed, Spirit _ ) 
			=> speed == Phase.Fast || speed == Phase.Slow;

		public string Name => "Replay Card for Cost" + Suffix;
		string Suffix => maxCost == int.MaxValue ? "" : $" [max cost:{maxCost}]";

		public string Text => Name;

		public async Task ActivateAsync( SpiritGameStateCtx ctx ) {

			int maxCardCost = System.Math.Min( this.maxCost, ctx.Self.Energy );
			var options = ctx.Self.UsedActions.OfType<PowerCard>() // can't use Discard pile because those cards are from prior rounds.  // !!! needs tests
				.Where(card=>ctx.Self.IsActiveDuring(ctx.GameState.Phase,card)) 
				.Where(card=>card.Cost<=maxCardCost)
				.ToArray();
			if(options.Length == 0) return;

			PowerCard factory = await ctx.Self.SelectPowerCard( "Select card to replay", options.Where( x => x.Cost <= maxCardCost ), CardUse.Replay, Present.Always );
			if(factory == null) return;

			ctx.Self.Energy -= factory.Cost;
			ctx.Self.AddActionFactory( factory );

		}

		readonly int maxCost;
	}



}
