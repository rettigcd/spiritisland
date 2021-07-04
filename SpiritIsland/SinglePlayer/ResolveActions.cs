using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.Core;

namespace SpiritIsland.SinglePlayer {

	class ResolveActions : IPhase {

		public string Prompt { get; private set; }
		public IOption[] Options { get; private set; }


		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Speed speed;
		readonly bool allowEarlyDone;
		IActionFactory selectedActionFactory;
		IAction action;
		string growthName;
		List<IActionFactory> matchingActionsFactories;

		public ResolveActions( Spirit spirit, GameState gameState, Speed speed, bool allowEarlyDone = false ) {
			this.spirit = spirit;
			this.gameState = gameState;
			this.speed = speed;
			this.allowEarlyDone = allowEarlyDone;
		}

		public void Initialize() {
			matchingActionsFactories = FindMatchingActions();

			if(matchingActionsFactories.Count == 0) {
				Done();
				return;
			}

			Prompt = "Select " + speed + " to resolve:";
			Options = GetActionFactoryOptions().ToArray();
			action = null;
		}

		public void Select( IOption option ) {
			if(option is TextOption txt) {
				if(txt.Text == "Done") Done();
				return;
			}

			// Select action or apply option
			if(option is IActionFactory factory) {
				selectedActionFactory = factory;
				this.growthName = selectedActionFactory.ToString(); // or .Name ?
				action = selectedActionFactory.Bind( spirit, gameState );
			} else {
				action.Select( option );
			}

			// next action or summarize options
			if(action.IsResolved) {
				// Next
				action.Apply();
				spirit.Resolve(selectedActionFactory);
				Initialize();
			} else {
				Prompt = growthName + " - " + action.Prompt;
				Options = action.Options;
			}
		}

		List<IActionFactory> FindMatchingActions() {
			return spirit.UnresolvedActionFactories
				.Where( x => x.Speed == speed )
				.ToList();
		}

		List<IOption> GetActionFactoryOptions() {
			var list = new List<IOption>();
			list.AddRange( matchingActionsFactories );
			if(allowEarlyDone) list.Add( new TextOption( "Done" ) );
			return list;
		}

		void Done() {
			var toFlush = spirit.UnresolvedActionFactories
				.Where( f => f.Speed == speed )
				.ToArray();
			foreach(var factory in toFlush)
				spirit.Resolve(factory);
			Console.WriteLine( $"{speed} Done! - Flushed {toFlush.Length} actions." );

			this.Complete?.Invoke();
		}

		public event Action Complete;

	}

}
