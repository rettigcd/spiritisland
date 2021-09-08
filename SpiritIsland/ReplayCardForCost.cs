using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ReplayCardForCost : IActionFactory {

		public ReplayCardForCost(int maxCost) {
			this.maxCost = maxCost;
		}

		public Speed Speed {
			get { return Speed.FastOrSlow; }
			set { throw new InvalidOperationException("you may not change the speed of a fast/slow"); }
		}

		public string Name => "Replay Card for cost";
		public string Text => Name;

		public IActionFactory Original => this;

		public async Task ActivateAsync( Spirit self, GameState _ ) {

			int maxCost = System.Math.Min( this.maxCost, self.Energy );
			var options = self.UsedActions.OfType<PowerCard>().ToArray();
			if(options.Length == 0) return;

			var factory = await self.Select( "Select card to replay", options.Where( x => x.Cost <= maxCost ).ToArray() );
			if(factory == null) return;

			self.Energy -= factory.Cost;
			self.AddActionFactory( factory );

		}

		readonly int maxCost;
	}


}
