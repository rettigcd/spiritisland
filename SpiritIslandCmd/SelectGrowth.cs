using System;
using System.Linq;
using SpiritIsland;

namespace SpiritIslandCmd {

	public class SelectGrowth : IPhase {
		
		public string Prompt => uiMap.ToPrompt();

		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Formatter formatter;

		public SelectGrowth(Spirit spirit,GameState gameState,Formatter formatter){
			this.spirit = spirit;
			this.gameState = gameState;
			this.formatter = formatter;
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

		public UiMap uiMap {get; set;}

		public void Initialize() {
			uiMap = new UiMap("Select Growth Option",spirit.GetGrowthOptions().Cast<IOption>(),formatter);
		}
	}

}
