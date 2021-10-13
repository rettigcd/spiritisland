using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

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

		public bool IsActiveDuring( Speed speed, CountDictionary<Element> _ ) 
			=> speed == Speed.Fast || speed == Speed.Slow;

		public string Name => "Replay Card for Cost" + Suffix;
		string Suffix => maxCost == int.MaxValue ? "" : $" [max cost:{maxCost}]";

		public string Text => Name;

		public async Task ActivateAsync( Spirit self, GameState _ ) {

			int maxCardCost = System.Math.Min( this.maxCost, self.Energy );
			var options = self.UsedActions.OfType<PowerCard>() // can't use Discard pile because those cards are from prior rounds.  // !!! needs tests
				.Where(card=>card.Cost<=maxCardCost)
				.ToArray();
			if(options.Length == 0) return;

			PowerCard factory = await self.SelectPowerCard( "Select card to replay", options.Where( x => x.Cost <= maxCardCost ), CardUse.Replay, Present.Always );
			if(factory == null) return;

			self.Energy -= factory.Cost;
			self.AddActionFactory( factory );

		}

		readonly int maxCost;
	}



}
