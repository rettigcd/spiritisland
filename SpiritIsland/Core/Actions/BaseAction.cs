using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	sealed public class BaseAction {

		public BaseAction( Stack<IDecision> decisions ) {
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

		public IOption[] Options {
			get{
				AutoSelectSingleOptions();
				return Options_Inner;
			}
		}

		public string Prompt { get {
				AutoSelectSingleOptions();
				return Prompt_Inner;
			}
		}

		string Prompt_Inner => Decisions.Count > 0 ? Decisions.Peek().Prompt : "-";
		IOption[] Options_Inner => Decisions.Count > 0
				? Decisions.Peek().Options
				: System.Array.Empty<IOption>();


		#region selection log

		/// <summary> Logs decisions made </summary>
		public string Selections => selections.Join( " > " );

		public readonly List<string> selections = new List<string>();

		#endregion

		#region private


		void AutoSelectSingleOptions() {
			var opt = Options_Inner;
			while(opt.Length == 1 && Decisions.Peek().AllowAutoSelect ) {
				Select_Inner( opt[0], true );
				opt = Options_Inner;
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
			if(auto) msg += "AUTO!";
			selections.Add( msg );

			decision.Select( selection );
		}

		readonly Stack<IDecision> Decisions;

		#endregion

	}

}
