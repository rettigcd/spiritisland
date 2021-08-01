using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public class BaseAction : IAction {

		protected BaseAction(Spirit spirit,GameState gameState){

			engine = new ActionEngine(spirit,gameState);
			this.spirit = spirit;

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
				int hiddenCount = spirit.decisions.Count - 1;
				if(hiddenCount>0)
					throw new System.InvalidOperationException($"'{Decisions.Peek().Prompt}' returned 0 options leaving {hiddenCount} decision unresolved. ");
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
				InitializeIfNeeded();
				return GetOptionsSkippingAutoSelectCheck();
			}
		}
		IOption[] GetOptionsSkippingAutoSelectCheck(){
			return Decisions.Count>0 
				? Decisions.Peek().Options 
				: System.Array.Empty<IOption>();
		}

		public string Prompt => Decisions.Count>0 ? Decisions.Peek().Prompt : "-";

		protected void InnerSelect(IOption option) {
			if(Decisions.Count == 0)
				throw new System.NotImplementedException();

			var descision = Decisions.Pop();
			selections.Add( descision.Prompt +":"+ option.Text );
			descision.Select( option );
		}


		protected readonly 
			GameState gameState;
		protected readonly ActionEngine engine;
		protected readonly Spirit spirit;

		protected Stack<IDecision> Decisions => spirit.decisions;
		public string Selections => selections.Join( " > " );
		public readonly List<string> selections = new List<string>();

	}

}
