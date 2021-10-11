﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class ReplayCardForCost : IActionFactory {

		public ReplayCardForCost(int maxCost) {
			this.maxCost = maxCost;
		}

		public bool IsActiveDuring( Speed speed, CountDictionary<Element> _ ) => speed == Speed.Fast || speed == Speed.Slow;
		public bool IsInactiveAfter( Speed speed ) => speed == Speed.Slow;

		public string Name => "Replay Card for Cost";
		public string Text => Name;

		public async Task ActivateAsync( Spirit self, GameState _ ) {

			int maxCost = System.Math.Min( this.maxCost, self.Energy );
			var options = self.UsedActions.OfType<PowerCard>().ToArray();
			if(options.Length == 0) return;

			PowerCard factory = await self.SelectPowerCard( "Select card to replay", options.Where( x => x.Cost <= maxCost ), CardUse.Replay, Present.Always );
			if(factory == null) return;

			self.Energy -= factory.Cost;
			self.AddActionFactory( factory );

		}

		public void UpdateFromSpiritState( CountDictionary<Element> elements ) {} // no effect

		readonly int maxCost;
	}



}
