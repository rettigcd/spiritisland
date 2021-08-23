using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.SinglePlayer {

	public class ResolveActions : IPhase {

		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Speed speed;
		readonly bool allowEarlyDone;

		public ResolveActions( Spirit spirit, GameState gameState, Speed speed, bool allowEarlyDone = false ) {
			this.spirit = spirit;
			this.gameState = gameState;
			this.speed = speed;
			this.allowEarlyDone = allowEarlyDone;
		}

		public void Initialize() {
			_ = ActAsync();
		}

		public async Task ActAsync() {
			List<IActionFactory> matchingActionFactories;

			while((matchingActionFactories = spirit.GetUnresolvedActionFactories( speed ).ToList()).Count> 0) {

				// -------------
				// Select Actions to resolve
				// -------------
				var factoryOptions = GetActionFactoryOptions( matchingActionFactories ).ToArray();
				var option = await spirit.SelectOption( "Select " + speed + " to resolve:", factoryOptions, Present.Always );
				if(TextOption.Done.Matches( option ))
					break;


				// if use clicked a slow card that was made fast, // slow card won't be in the options
				if(!factoryOptions.Contains( option ))
					// find the fast version of the slow card that was clicked
					option = factoryOptions.Cast<IActionFactory>()
						.First( factory => factory.Original == option );
				if(!factoryOptions.Contains( option ))
					throw new Exception( "Dude! - You selected something that wasn't an option" );



				var selectedActionFactory = (IActionFactory)option;
				// var growthName = selectedActionFactory.Name;
				await selectedActionFactory.ActivateAsync( spirit, gameState );

				spirit.RemoveUnresolvedFactory( selectedActionFactory);
			}

			Done();
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
