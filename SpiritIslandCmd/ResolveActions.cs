﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland;
using SpiritIsland.Core;

namespace SpiritIslandCmd {

	class ResolveActions : IPhase {
		
		public string Prompt => uiMap.ToPrompt();

		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Speed speed;
		readonly bool allowEarlyDone;
		readonly Formatter formatter;
		public UiMap uiMap {get; set;}
		IActionFactory selectedActionFactory;
		IAction action;
		string growthName;
		List<IActionFactory> matchingActionsFactories;

		public ResolveActions(Spirit spirit,GameState gameState,InvaderDeck deck, Speed speed, bool allowEarlyDone=false){
			this.spirit = spirit;
			this.gameState = gameState;
			this.speed = speed;
			this.allowEarlyDone = allowEarlyDone;
			formatter = new Formatter(spirit,gameState,deck);
		}

		public void Initialize() {
			matchingActionsFactories = FindMatchingActions();

			if(matchingActionsFactories.Count == 0) {
				Done();
				return;
			}

			uiMap = new UiMap( "Select " + speed + " to resolve:", GetActionFactoryOptions(), formatter );
			action = null;
		}

		public void Select(IOption option){
			if(option is TextOption txt){
				if(txt.Text == "Done") Done();
				return;
			}

			// Select action or apply option
			if(option is IActionFactory factory){
				selectedActionFactory = factory;
				this.growthName = selectedActionFactory.ToString(); // or .Name ?
				action = selectedActionFactory.Bind( spirit, gameState );
			} else {
				action.Select( option );
			}

			// next action or summarize options
			if(action.IsResolved){
				// Next
				action.Apply();
				selectedActionFactory.Resolved( spirit );
				Initialize();
			} else
				uiMap = new UiMap("Select Resolution for " + growthName+":",action.Options,formatter);
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
				.Where(f=>f.Speed==speed)
				.ToArray();
			foreach(var factory in toFlush)
				factory.Resolved(spirit);
			Console.WriteLine($"{speed} Done! - Flushed {toFlush.Length} actions.");

			this.Complete?.Invoke();
		}

		public event Action Complete;

	}

}
