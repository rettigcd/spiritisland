using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	class SelectGrowth : IPhase {

		readonly Spirit spirit;
		readonly GameState gameState;

		public void Initialize() {
			_ = ActAndTrigger();
		}

		public async Task ActAndTrigger() {
			await ActAsync();
			this.Complete?.Invoke();
		}

		public async Task ActAsync() {
			GrowthOption[] options = spirit.GetGrowthOptions();

			var option = (GrowthOption)await spirit.SelectGrowth( "Select Growth Option", options );

			int i=0;
			for(; i < options.Length; ++i)
				if(options[i].Equals( option )) break;

			spirit.Grow( gameState, i );

		}

		public SelectGrowth(Spirit spirit,GameState gameState){
			this.spirit = spirit;
			this.gameState = gameState;
		}

		public event Action Complete;

	}

}
