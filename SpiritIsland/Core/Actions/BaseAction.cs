using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	sealed public class BaseAction : IAction {

		public BaseAction(ActionEngine engine){
			this.Decisions = engine.Self.decisions;
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

		public string Prompt => Decisions.Count > 0 ? Decisions.Peek().Prompt : "-";

		/// <summary> Logs decisions made </summary>
		public string Selections => selections.Join( " > " );

		public readonly List<string> selections = new List<string>();

		#region private

		bool initialized = false;

		void InitializeIfNeeded() {
			if(initialized) return;
			initialized = true;
			AutoSelectSingleOptions();
		}

		void AutoSelectSingleOptions() {
			var opt = GetOptionsSkippingAutoSelectCheck();
			while(opt.Length == 1)
			{
				InnerSelect( opt[0] );
				opt = GetOptionsSkippingAutoSelectCheck();
			}
			if(opt.Length == 0)
			{
				int hiddenCount = Decisions.Count - 1;
				if(hiddenCount > 0)
					throw new System.InvalidOperationException( $"'{Decisions.Peek().Prompt}' returned 0 options leaving {hiddenCount} decision unresolved. " );
			}
		}

		IOption[] GetOptionsSkippingAutoSelectCheck(){
			return Decisions.Count>0 
				? Decisions.Peek().Options 
				: System.Array.Empty<IOption>();
		}

		void InnerSelect(IOption selection) {
			if(Decisions.Count == 0)
				throw new System.NotImplementedException();

			var decision = Decisions.Pop();
			if(!decision.Options.Contains(selection))
				throw new ArgumentException("You can't select an option that isn't there.");

			string msg = decision.Prompt + "(" + decision.Options.Select(o=>o.Text).Join(",") + "):" + selection.Text;
			selections.Add( msg );

			decision.Select( selection );
		}

		readonly Stack<IDecision> Decisions;

		#endregion

	}

}
