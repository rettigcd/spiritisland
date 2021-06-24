using SpiritIsland;
using System;
using System.Linq;

namespace SpiritIslandCmd {
	public class ResolveGrowth : IPhase {
		
		readonly Spirit spirit;
		readonly GameState gameState;

		public ResolveGrowth(Spirit spirit,GameState gameState){
			this.spirit = spirit;
			this.gameState = gameState;
		}

		public string Prompt { get; private set; }

		public event Action Complete;
		

		public bool Handle( string cmd ) {
			if(!int.TryParse(cmd,out int option1Based)) return false;
			int option = option1Based-1;
			if( action == null ){
				var growthAction = spirit.UnresolvedActions[option];
				this.growthName = growthAction.ToString();
				action = growthAction.Bind(spirit,gameState);

				int i=0;
				Prompt = "Select Resolution for " + growthName + " : " + action.Options
					.Select(o=>"\r\n"+(++i).ToString()+" : "+o.Text)
					.Join("");

			} else {
				action.Select(action.Options[option]);
				if(action.IsResolved){
					action.Apply();
					Initialize(); // hack
				} else {
					int i=0;
					Prompt = "Select Resolution for " + growthName + " : " + action.Options
						.Select(o=>"\r\n"+(++i).ToString()+" : "+o.Text)
						.Join("");
				}
			}
			return true;
		}

		public void Initialize() {
			if(spirit.UnresolvedActions.Count==0){
				// !!! collect energy
				this.Complete?.Invoke();
				return;
			}
			int i=0;
			Prompt = "Select Growth to resolve:" + spirit.UnresolvedActions
				.Select( x=>"\r\n"+(++i).ToString()+" : "+((GrowthAction)x).ShortDescription )
				.Join("");
			action = null;
		}
		IAction action;
		string growthName;
	}

}
