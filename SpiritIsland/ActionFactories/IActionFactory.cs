using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IActionFactory : IOption {

		Task ActivateAsync( SpiritGameStateCtx ctx );

		bool IsActiveDuring( Speed speed, CountDictionary<Element> elements );

		string Name { get; }
			
	}

	public interface IFlexibleSpeedActionFactory : IActionFactory {
		Speed Speed { get; }
		SpeedOverride OverrideSpeed { get; set; }
	}

	public class SpeedOverride {
		public SpeedOverride(Speed speed, string source ) {
			Speed = speed;
			Source = source;
		}
		public Speed Speed { get; }
		public string Source { get; }
	}

}