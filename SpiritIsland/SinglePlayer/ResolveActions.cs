using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class ResolveActions {

		public ResolveActions( Spirit spirit, GameState gameState, Phase speed, bool allowEarlyDone = false ) {

			this.speed = speed;
			this.present = allowEarlyDone ? Present.Done : Present.Always;

			var cause = speed switch {
				Phase.Fast => Cause.Power,
				Phase.Slow => Cause.Power,
				Phase.Growth => Cause.Growth,
				_ => throw new System.InvalidOperationException("Can't resolve actions of speed = "+speed)
			};

			this.ctx = new SpiritGameStateCtx( spirit, gameState, cause );
			this.Spirit = ctx.Self;

		}

		public Task ActAsync() => Spirit.ResolveActions( speed, present, ctx );

		#region private

		readonly Phase speed;
		readonly Present present;
		readonly SpiritGameStateCtx ctx;
		readonly Spirit Spirit;

		#endregion

	}

}
