using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	class SelectGrowth {

		public SelectGrowth( Spirit spirit, GameState gameState ) {
			this.spirit = spirit;
			this.gameState = gameState;
		}

		public async Task ActAsync() {
			var (allGrowthOptions,count) = spirit.GetGrowthOptions();

			List<GrowthOption> currentOptions = allGrowthOptions.ToList();

			while(count-->0) {
				var option = (GrowthOption)await spirit.SelectGrowth( "Select Growth Option", currentOptions.ToArray() );
				currentOptions.Remove(option);
				spirit.Grow( gameState, option );
			}

		}

		#region private
		readonly Spirit spirit;
		readonly GameState gameState;
		#endregion

	}

}
