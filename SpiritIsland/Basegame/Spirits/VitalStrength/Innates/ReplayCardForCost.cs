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

		public Task ActivateAsync( Spirit self, GameState _ ) {
			return self.SelectCardToReplayForCost( maxCost, self.UsedActions.OfType<PowerCard>().ToArray() );
		}

		readonly int maxCost;
	}


}
