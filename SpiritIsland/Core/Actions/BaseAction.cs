using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {
	public class BaseAction : IAction {

		protected BaseAction(GameState gameState){
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
				int hiddenCount = engine.decisions.Count - 1;
				if(hiddenCount>0)
					throw new System.InvalidOperationException($"'{engine.decisions.Peek().Prompt}' returned 0 options leaving {hiddenCount} decision unresolved. ");
			}
		}

		public bool IsResolved => Options.Length == 0;

		public void Apply() {
			foreach(var move in engine.actions)
				move.Apply( gameState );
		}

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
			return engine.decisions.Count>0 
				? engine.decisions.Peek().Options 
				: new IOption[0];
		}

		protected void InnerSelect(IOption option) {
			if(engine.decisions.Count == 0)
				throw new System.NotImplementedException();

			var descision = engine.decisions.Pop();
			selections.Add( descision.Prompt +":"+ option.Text );
			descision.Select( option, engine );
		}

		public readonly List<string> selections = new List<string>();
		public string Selections => selections.Join(" > ");

		protected readonly GameState gameState;
		protected readonly ActionEngine engine = new ActionEngine();
	}

}
