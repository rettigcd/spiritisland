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
		public IDecision GetCurrent() => WaitForNextDecisionAndCacheIt;

		IDecisionMaker WaitForNextDecisionAndCacheIt => userAccessedDecision ??= WaitForNextActiveDecision();
		IDecisionMaker userAccessedDecision;

		public bool IsResolved => acitveDecisionMaker == null;

		public void Choose( string text ) => Choose( GetCurrent().Options.VerboseSingle( o => o.Text == text ) );

		public void Choose(IOption selection) {
			var poppedDecision = WaitForNextDecisionAndCacheIt;
			this.acitveDecisionMaker = null;
			this.userAccessedDecision = null;

			if(!poppedDecision.Options.Contains( selection ))
				throw new ArgumentException( "You can't select an option that isn't there." );

			Log( selection, poppedDecision, false );

			poppedDecision.Select( selection );
		}

		#region selection log

		void Log( IOption selection, IDecisionMaker decision, bool auto ) {
			string msg = decision.Prompt + "(" + decision.Options.Select( o => o.Text ).Join( "," ) + "):" + selection.Text;
			if(auto) msg += " AUTO";
			selections.Add( msg );
		}

		/// <summary> Logs decisions made </summary>
		public string Selections => selections.Join( " > " );

		public readonly List<string> selections = new List<string>();

		#endregion

		public void Push(IDecisionMakerPlus decision){
			if(decision == null)
				throw new ArgumentNullException(nameof(decision));
			if(acitveDecisionMaker != null ) 
				throw new InvalidOperationException("decision already pending");

			if(decision.Options.Length==1 && decision.AllowAutoSelect) {
				decision.Select(decision.Options[0]);
				Log( decision.Options[0], decision, true );
			} else if( decision.Options.Length > 0 ) {
				acitveDecisionMaker = decision;
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
