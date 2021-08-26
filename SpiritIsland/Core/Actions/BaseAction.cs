using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SpiritIsland {

	sealed public class BaseAction : IDecisionStream {

		/// <summary>
		/// Blocks and waits for there to be a decision. 
		/// Don't call unless you are willing to block.
		/// </summary>
		public IDecision GetCurrent() => WaitForNextDecisionAndCacheIt.Decision;

		IDecisionMaker WaitForNextDecisionAndCacheIt => userAccessedDecision ??= WaitForNextActiveDecision();
		IDecisionMaker userAccessedDecision;

		public bool IsResolved => acitveDecisionMaker == null;

		public void Choose( string text ) => Choose( GetCurrent().Options.First( o => o.Text == text ) ); // not single because some options appear twice

		public void Choose(IOption selection) {
			var poppedDecisionMaker = WaitForNextDecisionAndCacheIt;
			var poppedDecision = poppedDecisionMaker.Decision;
			this.acitveDecisionMaker = null;
			this.userAccessedDecision = null;

			if(!poppedDecision.Options.Contains( selection ))
				throw new ArgumentException( selection.Text + " not found in options("+ poppedDecision.Options.Select(x=>x.Text).Join(",") + ")" );

			Log( selection, poppedDecisionMaker.Decision, false );

			poppedDecisionMaker.Select( selection );
		}

		#region selection log

		void Log( IOption selection, IDecision decision, bool auto ) {
			string msg = decision.Prompt + "(" + decision.Options.Select( o => o.Text ).Join( "," ) + "):" + selection.Text;
			if(auto) msg += " AUTO";
			selections.Add( msg );
		}

		/// <summary> Logs decisions made </summary>
		public string Selections => selections.Join( " > " );

		public readonly List<string> selections = new List<string>();

		#endregion

		public void Push(IDecisionMaker decisionMaker){
			if(decisionMaker == null)
				throw new ArgumentNullException(nameof(decisionMaker));
			if(acitveDecisionMaker != null ) 
				throw new InvalidOperationException("decision already pending");

			var decision = decisionMaker.Decision;
			if(decision.Options.Length==1 && decision.AllowAutoSelect) {
				decisionMaker.Select( decision.Options[0]);
				Log( decision.Options[0], decision, true );
			} else if(decision.Options.Length > 0 ) {
				acitveDecisionMaker = decisionMaker;
				signal.Set();
			}
		}

		public void Clear(){ 
			acitveDecisionMaker = null;	// This does something that lets unit tests pass
		}

		#region private

		readonly AutoResetEvent signal = new AutoResetEvent( false );
		IDecisionMaker acitveDecisionMaker;
		IDecisionMaker WaitForNextActiveDecision() {
			signal.WaitOne();
			return acitveDecisionMaker;
		}

		#endregion

	}

}
