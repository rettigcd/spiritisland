using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {
	public class ChangeSpeed : IActionFactory {
		public Speed Speed {
			get => Speed.FastOrSlow;
			set => throw new InvalidOperationException();
		}

		public string Name => "Change Speed";

		public IActionFactory Original => this;

		public string Text => Name;

		public Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return new SpeedChanger( spirit, gameState, Speed.Fast, 2 ).Exec();
		}

	}
}
