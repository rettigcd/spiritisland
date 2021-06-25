using SpiritIsland;
using System;
using System.Linq;

namespace SpiritIslandCmd {
	public class SelectGrowth : IPhase {
		
		readonly Spirit spirit;
		readonly GameState gameState;

		public SelectGrowth(Spirit spirit,GameState gameState){
			this.spirit = spirit;
			this.gameState = gameState;
		}

		public string Prompt { get; private set; }

		public event Action Complete;

		public bool Handle( string cmd, int index ) {
			if(index != -1){
				spirit.Grow(gameState,index); // users enter 1-3 for index 0-2
				this.Complete?.Invoke();
				return true;
			}
			return false;
		}

		public void Initialize() {
			int i=0;
			Prompt = "Select Growth Option" + spirit
				.GetGrowthOptions()
				.Select(opt=>"\r\n"+(++i).ToString() + " : "+ opt.ToString())
				.Join("");
		}
	}

}
