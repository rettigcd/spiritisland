using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	// Change Speed - delayed.  They don't have to pick it immediately - similar to Lightning
	public class ChangeSpeed : IActionFactory {

		public bool CouldActivateDuring( Phase speed, Spirit _ ) => speed == Phase.Fast || speed == Phase.Slow;

		public string Name => "Change Speed";

		public string Text => Name;

		public Task ActivateAsync(SpiritGameStateCtx ctx) => new SpeedChanger( ctx.Self, ctx.GameState, Phase.Fast, 2 ).Exec();


	}

}
