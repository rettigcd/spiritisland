using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ReplayCard : IActionFactory {

		public ReplayCard( int maxCost ) {
			this.maxCost = maxCost;
		}

		public bool IsActiveDuring( Speed speed, CountDictionary<Element> _ ) => speed == Speed.Fast || speed == Speed.Slow;

		public string Name => $"Replay Card [max cost:{maxCost}]";
		public string Text => Name;

		public async Task ActivateAsync( Spirit self, GameState _ ) {

			var options = self.UsedActions	// used
				.OfType<PowerCard>()		// only power cards, not innates
				.Where(card=>card.Cost <= maxCost)
				.Where(card=>self.IsActiveDuring(self.LastSpeedRequested,card)) // if cards are played at a differnet speed, is that the speed we want to replay?
				.ToArray(); 
			if(options.Length == 0) return;

			PowerCard factory = await self.SelectPowerCard( "Select card to replay", options, CardUse.Replay, Present.Always );
			if(factory == null) return;

			self.AddActionFactory( factory );

		}

		readonly int maxCost;
	}



}
