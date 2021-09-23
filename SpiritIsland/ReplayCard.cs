using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class ReplayCard : IActionFactory {

		public ReplayCard( int maxCost ) {
			this.maxCost = maxCost;
		}

		public Speed Speed => DefaultSpeed;
		public Speed DefaultSpeed => Speed.FastOrSlow;
		public SpeedOverride OverrideSpeed {
			get => null;
			set => throw new InvalidOperationException( "you may not change the speed of a fast/slow" );
		}

		public string Name => $"Replay Card [max cost:{maxCost}]";
		public string Text => Name;

		public async Task ActivateAsync( Spirit self, GameState _ ) {

			var options = self.UsedActions	// used
				.OfType<PowerCard>()		// only power cards, not innates
				.Where(card=>card.Cost <= maxCost)
				.Where(card=>card.Speed == self.LastSpeedRequested) // if cards are played at a differnet speed, is that the speed we want to replay?
				.ToArray(); 
			if(options.Length == 0) return;

			PowerCard factory = await self.SelectPowerCard( "Select card to replay", options.ToArray() );
			if(factory == null) return;

			self.AddActionFactory( factory );

		}

		public void UpdateFromSpiritState( CountDictionary<Element> elements ) {} // no effect

		readonly int maxCost;
	}



}
