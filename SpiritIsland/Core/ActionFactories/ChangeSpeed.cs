﻿using System.Threading.Tasks;

namespace SpiritIsland {

    public class ChangeSpeed : IActionFactory {

		public ChangeSpeed(IActionFactory original, Speed newSpeed){
			Original = original;
			Speed = newSpeed;
		}
		public Speed Speed { get; }

		public string Name => Original.Name;

		public string Text => Original.Text;

		public Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return Original.ActivateAsync(spirit, gameState);
		}

		public IActionFactory Original { get; }

	}

}