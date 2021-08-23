using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	sealed public class BaseAction : IDecisionStream {

		public IDecision Current { get {
			AutoSelectSingleOptions();
			return Decision_Inner;
		} }

		IDecision Decision_Inner => Decisions.Count > 0 ? Decisions.Peek() : Decision.Null;

		public IOption[] Options => Current.Options;
		public string Prompt => Current.Prompt;

		public BaseAction( Stack<IDecisionPlus> decisions ) {
			this.Decisions = decisions;
		}

		public bool IsResolved => Options.Length == 0;

		public void Select(IOption option) {
			AutoSelectSingleOptions();
			Select_Inner( option, false );
			AutoSelectSingleOptions();
		}
		public void Select(string text)
			=> Select(Options.Single(o=>o.Text==text));

		#region selection log

		/// <summary> Logs decisions made </summary>
		public string Selections => selections.Join( " > " );

		public readonly List<string> selections = new List<string>();

		#endregion

		#region private


		void AutoSelectSingleOptions() {
			var opt = Decision_Inner.Options;
			while(opt.Length == 1 && Decisions.Peek().AllowAutoSelect ) {
				Select_Inner( opt[0], true );
				opt = Decision_Inner.Options;
			}
			if(opt.Length == 0) {
				int hiddenCount = Decisions.Count - 1;
				if(hiddenCount > 0)
					throw new System.InvalidOperationException( $"'{Decisions.Peek().Prompt}' returned 0 options leaving {hiddenCount} decision unresolved. " );
			}
		}

		void Select_Inner(IOption selection,bool auto) {
			if(Decisions.Count == 0)
				throw new System.NotImplementedException();

			var decision = Decisions.Pop();
			if(!decision.Options.Contains(selection))
				throw new ArgumentException("You can't select an option that isn't there.");

			string msg = decision.Prompt + "(" + decision.Options.Select(o=>o.Text).Join(",") + "):" + selection.Text;
			if(auto) msg += " AUTO!";
			selections.Add( msg );

			decision.Select( selection );
		}

		readonly Stack<IDecisionPlus> Decisions;

		#endregion

	}

}
