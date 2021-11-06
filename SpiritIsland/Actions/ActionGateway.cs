using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpiritIsland {

	sealed public class ActionGateway : IUserPortal, IEnginePortal {

		#region IUserPortal

		/// <summary>
		/// Blocks and waits for there to be a decision. 
		/// Don't call unless you are willing to block.
		/// </summary>
		public IDecision GetCurrent() => WaitForNextDecisionAndCacheIt().Decision;

		public event Action<IDecision> NewWaitingDecision;

		IDecisionMaker WaitForNextDecisionAndCacheIt() {
			if(userAccessedDecision == null) {
				signal.WaitOne(); // !!! When we get too much blight, this will lock up the UI - replace with a timed signal
				userAccessedDecision = activeDecisionMaker;
			}
			return userAccessedDecision;
		}

		public bool WaitForNextDecision( int milliseconds ) {
			if(signal.WaitOne( milliseconds )) {
				userAccessedDecision = activeDecisionMaker;
				return true;
			}
			return false;
		}

		public bool IsResolved => activeDecisionMaker == null;

		public void Choose( string text ) {
			var current = GetCurrent();
			var choice = current.Options.FirstOrDefault( o => o.Text == text );
			if(choice == null)
				throw new ArgumentOutOfRangeException(nameof(text),"sequence ["+current.Options.Select(x=>x.Text).Join(",")+"]does not contain option: "+text);
			Choose( choice ); // not single because some options appear twice
		}

		public void Choose(IOption selection) {
			var poppedDecisionMaker = WaitForNextDecisionAndCacheIt();
			var poppedDecision = poppedDecisionMaker.Decision;
			this.activeDecisionMaker = null;
			this.userAccessedDecision = null;

			if(!poppedDecision.Options.Contains( selection ))
				throw new ArgumentException( selection.Text + " not found in options("+ poppedDecision.Options.Select(x=>x.Text).Join(",") + ")" );

			Log( selection, poppedDecision, false );

			poppedDecisionMaker.Select( selection ); // ####
		}

		public void GoBackToBeginningOfRound() {
			var poppedDecisionMaker = WaitForNextDecisionAndCacheIt();
			this.activeDecisionMaker = null;
			this.userAccessedDecision = null;
			poppedDecisionMaker.IssueCommand( GameStateCommand.ReturnToBeginningOfRound );
		}

		#endregion

		/// <summary>
		/// Caller presents a decision to the Gateway and waits for the gateway to return an choice.
		/// </summary>
		public Task<T> Decision<T>( Decision.TypedDecision<T> originalDecision ) where T : class, IOption {
			if(originalDecision == null) throw new ArgumentNullException( nameof( originalDecision ) );
			if(activeDecisionMaker != null) throw new InvalidOperationException( "decision already pending" );

			var promise = new TaskCompletionSource<T>();
			var decisionMaker = new ActionHelper<T>( originalDecision, promise );
			var decision = decisionMaker.Decision;

			if(decision.Options.Length == 0)
				// Auto-Select NULL
				promise.TrySetResult( null );

			else if(decision.Options.Length == 1 && decision.AllowAutoSelect) {
				// Auto-Select Single
				decisionMaker.Select( decision.Options[0] );
				Log( decision.Options[0], decision, true );

			} else {
				activeDecisionMaker = decisionMaker;
				signal.Set();
				NewWaitingDecision?.Invoke(decision);
			}
			return promise.Task;
		}

		#region selection log / private

		void Log( IOption selection, IDecision decision, bool auto ) {
			string msg = decision.Prompt + "(" + decision.Options.Select( o => o.Text ).Join( "," ) + "):" + selection.Text;
			if(auto) msg += " AUTO";
			selections.Add( msg );
		}

		public readonly List<string> selections = new List<string>();

		readonly AutoResetEvent signal = new AutoResetEvent( false );
		IDecisionMaker activeDecisionMaker;
		IDecisionMaker userAccessedDecision;

		#endregion

		interface IDecisionMaker {
			public IDecisionPlus Decision { get; }
			public void Select(IOption option);
			public void IssueCommand( GameStateCommand cmd );
		}

		class ActionHelper<T> : IDecisionMaker where T : class, IOption {

			public IDecisionPlus Decision { get; }

			public ActionHelper( IDecisionPlus decision, TaskCompletionSource<T> promise ) {
				Decision = decision;
				this.promise = promise;
			}

			public void Select( IOption selection ) {
				if( TextOption.Done.Matches( selection ) || selection is not T tt )
					promise.TrySetResult( null );
				else if(Decision.Options.Contains( selection ))
					promise.TrySetResult( tt );
				else
					promise.TrySetException( new Exception( $"{selection.Text} not found in options" ) );
			}

			public void IssueCommand( GameStateCommand cmd ) {
				promise.TrySetException( new GameStateCommandException(cmd) );
			}

			readonly TaskCompletionSource<T> promise;

		}

	}

}
