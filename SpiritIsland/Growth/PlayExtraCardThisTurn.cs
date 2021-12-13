﻿using System.Threading.Tasks;

namespace SpiritIsland {

	public class PlayExtraCardThisTurn : GrowthActionFactory {

		readonly int count;
		public PlayExtraCardThisTurn(int count ) {
			this.count = count;
		}

		public override Task ActivateAsync( SpiritGameStateCtx ctx ) {
			ctx.Self.tempCardPlayBoost++;
			return Task.CompletedTask;
		}

		public override string Name => $"PlayExtraCardThisTurn({count})";

	}

}
