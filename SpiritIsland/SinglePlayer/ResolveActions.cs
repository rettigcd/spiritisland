using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class ResolveActions {

		public ResolveActions( Spirit spirit, GameState gameState, Speed speed, bool allowEarlyDone = false ) {

			this.speed = speed;
			this.present = allowEarlyDone ? Present.Done : Present.Always;

			var cause = speed switch {
				Speed.Fast => Cause.Power,
				Speed.Slow => Cause.Power,
				Speed.FastOrSlow => Cause.Power,
				Speed.Growth => Cause.Growth,
				_ => throw new System.InvalidOperationException("Can't resolve actions of speed = "+speed)
			};

			this.ctx = new SpiritGameStateCtx( spirit, gameState, cause);
		}

		public Task ActAsync() => ctx.Self.ResolveActions( speed, present, ctx );

		#region private

		readonly Speed speed;
		readonly Present present;
		readonly SpiritGameStateCtx ctx;

		#endregion

	}

}
