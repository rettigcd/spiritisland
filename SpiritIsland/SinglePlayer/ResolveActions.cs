using SpiritIsland;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class ResolveActions {

		public ResolveActions( Spirit spirit, GameState gameState, Speed speed, bool allowEarlyDone = false ) {
			this.spirit = spirit;
			this.gameState = gameState;
			this.speed = speed;
			this.present = allowEarlyDone ? Present.Done : Present.Always;
		}

		public async Task ActAsync() {
			IActionFactory[] factoryOptions;

			while((factoryOptions = spirit.GetAvailableActions( speed ).ToArray()).Length> 0) {

				// -------------
				// Select Actions to resolve
				// -------------
				IActionFactory option = await spirit.SelectFactory( "Select " + speed + " to resolve:", factoryOptions, present );
				if(option == null)
					break;

				// if user clicked a slow card that was made fast, // slow card won't be in the options
				if(!factoryOptions.Contains( option ))
					// find the fast version of the slow card that was clicked
					option = factoryOptions.Cast<IActionFactory>()
						.First( factory => factory == option );

				if(!factoryOptions.Contains( option ))
					throw new Exception( "Dude! - You selected something that wasn't an option" );

				await spirit.TakeAction( (IActionFactory)option, gameState );
			}

		}

		#region private
		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Speed speed;
		readonly Present present;
		#endregion

	}

}
