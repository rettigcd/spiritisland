using System;
using System.Linq;

namespace SpiritIsland.SinglePlayer {

	class SelectGrowth : IPhase {

		public IDecision Current {get; private set; }
		public bool IsResolved => spirit.Action.IsResolved;

		readonly Spirit spirit;
		readonly GameState gameState;

		public SelectGrowth(Spirit spirit,GameState gameState){
			this.spirit = spirit;
			this.gameState = gameState;
			Current = new Decision {
				Prompt = "Select Growth Option",
				Options = spirit.GetGrowthOptions().Cast<IOption>().ToArray()
			};
		}

		public event Action Complete;

		public bool AllowAutoSelect { get; set; } = true;

		public void Choose(IOption option){
			var options = Current.Options;
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
