﻿
using System.Threading.Tasks;

namespace SpiritIsland.Core {
	public class ReclaimAll : GrowthActionFactory {

		public override Task Activate( ActionEngine engine ) {
			var spirit = engine.Self;
			spirit.Hand.AddRange( spirit.DiscardPile );
			spirit.DiscardPile.Clear();
			return Task.CompletedTask;
		}

	}

}
