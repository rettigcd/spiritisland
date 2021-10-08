using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class SelectGrowth {

		readonly ResolveActions resolveGrowth;

		public SelectGrowth( Spirit spirit, GameState gameState ) {
			this.spirit = spirit;
			this.gameState = gameState;
			resolveGrowth = new ResolveActions( spirit, gameState, Speed.Growth, false );
		}

		public async Task ActAsync() {
			var (allGrowthOptions,count) = spirit.GetGrowthOptions();

			List<GrowthOption> remainingOptions = allGrowthOptions.ToList();

			while(count-->0) {
				var currentOptions = remainingOptions.Where(o=>o.GainEnergy+spirit.Energy>=0).ToArray();
				GrowthOption option = (GrowthOption)await spirit.Select( "Select Growth Option", currentOptions, Present.Always );
				remainingOptions.Remove(option);
				spirit.Grow( gameState, option );
				await resolveGrowth.ActAsync();
			}

			await spirit.TriggerEnergyElementsAndReclaims();

		}

		#region private
		readonly Spirit spirit;
		readonly GameState gameState;
		#endregion

	}

}
