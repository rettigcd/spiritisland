using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	// Change Speed - delayed.  They don't have to pick it immediately - similar to Lightning
	public class ChangeSpeed : IActionFactory {

		public Speed Speed => Speed;
		public Speed DefaultSpeed => Speed.FastOrSlow;

		public SpeedOverride OverrideSpeed { get => null; set => throw new InvalidOperationException(); }

		public string Name => "Change Speed";

//		public IActionFactory Original => this;

		public string Text => Name;

		public Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return new SpeedChanger( spirit, gameState, Speed.Fast, 2 ).Exec();
		}

		public void UpdateFromSpiritState( CountDictionary<Element> elements ) {} // no effect
	}
}
