using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IActionFactory : IOption {

		Task ActivateAsync( SpiritGameStateCtx ctx );

		bool CouldActivateDuring( Phase speed, Spirit spirit );

		string Name { get; }
			
	}

	public interface IFlexibleSpeedActionFactory : IActionFactory {
		Phase Speed { get; }
		SpeedOverride OverrideSpeed { get; set; }
	}

	public class SpeedOverride {
		public SpeedOverride(Phase speed, string source ) {
			Speed = speed;
			Source = source;
		}
		public Phase Speed { get; }
		public string Source { get; }
	}

}