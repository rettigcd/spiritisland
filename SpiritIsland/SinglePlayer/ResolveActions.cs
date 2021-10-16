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

		public Task ActAsync() => spirit.ResolveActions( gameState, speed, present );

		#region private
		readonly Spirit spirit;
		readonly GameState gameState;
		readonly Speed speed;
		readonly Present present;
		#endregion

	}

}
