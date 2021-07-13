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
			if(action == null && TextOption.Done.Matches(option)){ 
				Done(); 
				return;
			}

			// Select action or apply option
			if(action == null) {
				IActionFactory factory = (IActionFactory)option;
				selectedActionFactory = factory;
				this.growthName = selectedActionFactory.Name;
				action = selectedActionFactory.Bind( spirit, gameState );
			} else {
				action.Select( option );
			}

			// next action or summarize options
			if(action.IsResolved) {
				// Next
				spirit.Resolve( selectedActionFactory );
				Initialize();
			} else {
				Prompt = growthName + " - " + action.Prompt;
				Options = action.Options;
			}
		}

		List<IActionFactory> FindMatchingActions() {
			return spirit.GetUnresolvedActionFactories(speed)
				.ToList();
		}

		List<IOption> GetActionFactoryOptions() {
			var list = new List<IOption>();
			list.AddRange( matchingActionsFactories );
			if(allowEarlyDone) list.Add( TextOption.Done );
			return list;
		}

		void Done() {
			int numberFlushed = spirit.Flush(speed);
			Console.WriteLine( $"{speed} Done! - Flushed {numberFlushed} actions." );

			this.Complete?.Invoke();
		}

		public event Action Complete;

	}

}
