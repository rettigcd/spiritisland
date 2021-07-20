using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {
	public class BaseAction : IAction {

		protected BaseAction(Spirit spirit,GameState gameState){

			engine = new ActionEngine(spirit,gameState,decisions);

			this.gameState = gameState;
		}

		bool initialized = false;

		void InitializeIfNeeded(){
			if(initialized) return;
			initialized = true;
			AutoSelectSingleOptions();
		}

		void AutoSelectSingleOptions() {
			var opt = GetOptionsSkippingAutoSelectCheck();
			while (opt.Length == 1) {
				InnerSelect(opt[0]);
				opt = GetOptionsSkippingAutoSelectCheck();
			}
			if(opt.Length == 0){
				int hiddenCount = decisions.Count - 1;
				if(hiddenCount>0)
					throw new System.InvalidOperationException($"'{decisions.Peek().Prompt}' returned 0 options leaving {hiddenCount} decision unresolved. ");
			}
		}

		public bool IsResolved => Options.Length == 0;

		public void Select(IOption option) {
			InitializeIfNeeded();
			InnerSelect(option);
			AutoSelectSingleOptions();
		}

		public IOption[] Options {
			get{
//				System.Threading.Thread.Sleep(100);// !!!!!!!!!!!!!!!!!!! let other thread catch up
				InitializeIfNeeded();
				return GetOptionsSkippingAutoSelectCheck();
			}
		}
		IOption[] GetOptionsSkippingAutoSelectCheck(){
			return decisions.Count>0 
				? decisions.Peek().Options 
				: System.Array.Empty<IOption>();
		}

		public string Prompt => decisions.Count>0 ? decisions.Peek().Prompt : "-";

		protected void InnerSelect(IOption option) {
			if(decisions.Count == 0)
				throw new System.NotImplementedException();

			var descision = decisions.Pop();
			selections.Add( descision.Prompt +":"+ option.Text );
			descision.Select( option );
		}

		public readonly List<string> selections = new List<string>();
		public string Selections => selections.Join(" > ");

		protected readonly GameState gameState;
		protected readonly ActionEngine engine;
		protected readonly Stack<IDecision> decisions = new Stack<IDecision>();

	}

}
