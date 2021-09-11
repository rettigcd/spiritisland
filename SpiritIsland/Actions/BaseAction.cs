using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

		public void Choose( string text ) {
			var current = GetCurrent();
			var choice = current.Options.FirstOrDefault( o => o.Text == text );
			if(choice == null)
				throw new ArgumentOutOfRangeException(nameof(text),"sequence ["+current.Options.Select(x=>x.Text).Join(",")+"]does not contain option: "+text);
			Choose( choice ); // not single because some options appear twice
		}

		public void Choose(IOption selection) {
			var poppedDecisionMaker = WaitForNextDecisionAndCacheIt;
			var poppedDecision = poppedDecisionMaker.Decision;
			this.acitveDecisionMaker = null;
			this.userAccessedDecision = null;

			if(!poppedDecision.Options.Contains( selection ))
				throw new ArgumentException( selection.Text + " not found in options("+ poppedDecision.Options.Select(x=>x.Text).Join(",") + ")" );

			Log( selection, poppedDecisionMaker.Decision, false );

			poppedDecisionMaker.Select( selection ); // ####
		}

		public Task<T> Decision<T>( Decision.TypedDecision<T> originalDecision ) where T : class, IOption {
			if(originalDecision == null) throw new ArgumentNullException( nameof( originalDecision ) );
			if(acitveDecisionMaker != null) throw new InvalidOperationException( "decision already pending" );

			var promise = new TaskCompletionSource<T>();
			var decisionMaker = new ActionHelper<T>( originalDecision, promise );
			var decision = decisionMaker.Decision;

			if(decision.Options.Length == 0)

				promise.TrySetResult( null );

			else if(decision.Options.Length == 1 && decision.AllowAutoSelect) {

				decisionMaker.Select( decision.Options[0] ); // ####
				Log( decision.Options[0], decision, true );

			} else {
				acitveDecisionMaker = decisionMaker;
				signal.Set();
			}
			return promise.Task;
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

		static public void Select<T>( IOption option, IOption[] options, TaskCompletionSource<T> promise ) where T:class,IOption {
			if(/*present == Present.Done &&*/ TextOption.Done.Matches( option ))
				promise.TrySetResult( null );
			else if(options.Contains( option ))
				promise.TrySetResult( (T)option );
			else
				promise.TrySetException( new Exception( $"{option.Text} not found in options" ) );
		}

		class ActionHelper<T> : IDecisionMaker where T : class, IOption {

			public IDecisionPlus Decision { get; }

			public ActionHelper( IDecisionPlus decision, TaskCompletionSource<T> promise ) {
				Decision = decision;
				this.promise = promise;
			}

			public void Select( IOption selection ) {
				if(/*present == Present.Done &&*/ TextOption.Done.Matches( selection ))
					promise.TrySetResult( null );
				else if(Decision.Options.Contains( selection ))
					promise.TrySetResult( (T)selection );
				else
					promise.TrySetException( new Exception( $"{selection.Text} not found in options" ) );
			}

			readonly TaskCompletionSource<T> promise;

		}

	}

}
