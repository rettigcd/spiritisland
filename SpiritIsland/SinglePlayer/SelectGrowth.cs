using System;
using System.Linq;
using SpiritIsland;

namespace SpiritIslandCmd {

	class SelectGrowth : IPhase {

		public string Prompt => "Select Growth Option";
		public IOption[] Options => spirit.GetGrowthOptions().Cast<IOption>().ToArray();

		readonly Spirit spirit;
		readonly GameState gameState;

		public SelectGrowth(Spirit spirit,GameState gameState){
			this.spirit = spirit;
			this.gameState = gameState;
		}

		public event Action Complete;

		public void Select(IOption option){
			var options = spirit.GetGrowthOptions();
			for(int i=0;i<options.Length;++i){
				if(options[i].Equals(option)){
					spirit.Grow(gameState,i);
					this.Complete?.Invoke();
					return;
				}
			}
			throw new Exception("growth option not found");
		}

		public void Initialize() {}
	}

}
