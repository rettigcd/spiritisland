using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	// Change Speed - delayed.  They don't have to pick it immediately - similar to Lightning
	public class ChangeSpeed : IActionFactory {

		public bool IsActiveDuring( Speed speed, CountDictionary<Element> _ ) => speed == Speed.Fast || speed == Speed.Slow;

		public string Name => "Change Speed";

		public string Text => Name;

		public Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return new SpeedChanger( spirit, gameState, Speed.Fast, 2 ).Exec();
		}

	}

}
