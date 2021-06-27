using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.Core;

namespace SpiritIslandCmd {

	class ResolveActions : IPhase {
		
		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Speed speed;
		readonly bool allowEarlyDone;
		readonly Formatter formatter;

		public ResolveActions(Spirit spirit,GameState gameState,InvaderDeck deck, Speed speed, bool allowEarlyDone=false){
			this.spirit = spirit;
			this.gameState = gameState;
			this.speed = speed;
			this.allowEarlyDone = allowEarlyDone;
			formatter = new Formatter(spirit,gameState,deck);
		}

		public void Initialize() {
			matchingActions = FindMatchingActions();

			if(matchingActions.Count == 0) {
				Done();
				return;
			}

			Prompt = SummarizeActions();
			action = null;
		}

		public string Prompt { get; private set; }

		public event Action Complete;

		public bool Handle( string cmd,int index ) {
			if(allowEarlyDone && cmd=="d"){ Done(); return true; }
			return index >= 0 
				&& (action == null
					? SelectAction( index )
					: SelectOption( index )
				);
		}

		bool SelectAction( int index ) {
			if(index>=matchingActions.Count) return false;
			selectedAction = matchingActions[index];
			this.growthName = selectedAction.ToString(); // or .Name ?
			action = selectedAction.Bind( spirit, gameState );
			if(action.IsResolved){
				action.Apply();
				selectedAction.Resolved( spirit );
				Initialize(); // Next
			} else
				Prompt = SummarizeOptions();
			return true;
		}

		bool SelectOption( int index ) {
			if(index>=action.Options.Length) return false;
			action.Select( action.Options[index] );
			if(action.IsResolved) {
				action.Apply();
				selectedAction.Resolved( spirit );
				Initialize(); // Next
			} else
				Prompt = SummarizeOptions();
			return true;
		}

		List<IActionFactory> FindMatchingActions() {
			return spirit.UnresolvedActionFactories
				.Where( x => x.Speed == speed )
				.ToList();
		}

		void Done() {
			var toFlush = spirit.UnresolvedActionFactories
				.Where(f=>f.Speed==speed)
				.ToArray();
			foreach(var factory in toFlush)
				factory.Resolved(spirit);
			Console.WriteLine($"{speed} Done! - Flushed {toFlush.Length} actions.");

			this.Complete?.Invoke();
		}

		string SummarizeActions() {
			int i = 0;
			var optionStrings = matchingActions
				.Select( x => "\r\n\t" + (++i).ToString() + " : " + x.Name )
				.ToList();
			if(allowEarlyDone)
				optionStrings.Add( "\r\n\td : done" );
			return "Select "+speed+" to resolve:" + optionStrings.Join( "" );
		}

		string SummarizeOptions() {
			int i = 0;
			return "Select Resolution for " + growthName + " : " + action.Options
				.Select( o => "\r\n\t" + (++i).ToString() + " : " + formatter.Format(o) )
				.Join( "" );
		}

		IActionFactory selectedAction;
		IAction action;
		string growthName;
		List<IActionFactory> matchingActions;
	}
}
