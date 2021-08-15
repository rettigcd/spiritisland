using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;

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

		public ResolveActions( Spirit spirit, GameState gameState, Speed speed, bool allowEarlyDone = false ) {
			this.spirit = spirit;
			this.gameState = gameState;
			this.speed = speed;
			this.allowEarlyDone = allowEarlyDone;
		}

		public void Initialize() {
			var matchingActionFactories = spirit.GetUnresolvedActionFactories( speed ).ToList();

			if(matchingActionFactories.Count == 0) {
				Done();
				return;
			}

			Prompt = "Select " + speed + " to resolve:";
			Options = GetActionFactoryOptions( matchingActionFactories ).ToArray();
			action = null;
		}

		public void Select( IOption option ) {
			if(action == null && TextOption.Done.Matches(option)){ 
				Done(); 
				return;
			}

			// Select action or apply option
			if(action == null) {
				// if use clicked a slow card that was made fast
				// slow card won't be in the options
				if(!Options.Contains( option ))
					// find the fast version of the slow card that was clicked
					option = Options.Cast<IActionFactory>()
						.First(factory=>factory.Original==option);

				selectedActionFactory = (IActionFactory)option;

				if(!Options.Contains(option))
					throw new Exception("Dude! - You selected something that wasn't an option");

				selectedActionFactory = (IActionFactory)option;
				this.growthName = selectedActionFactory.Name;
				var engine = new ActionEngine( spirit, gameState );
				selectedActionFactory.Activate( engine );
				action = spirit.Action;
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

		List<IOption> GetActionFactoryOptions( List<IActionFactory> actionFactories ) {
			var list = actionFactories.Cast<IOption>().ToList();
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
