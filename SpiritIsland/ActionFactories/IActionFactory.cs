using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IActionFactory : IOption {

		Task ActivateAsync(Spirit spirit, GameState gameState);

		bool IsActiveDuring( Speed speed );

		bool IsInactiveAfter( Speed speed );

		string Name { get; }

		// Used by Innates to set Triggered / Not Triggered
		// Used by certain innates and Cards to determine speed
		void UpdateFromSpiritState( CountDictionary<Element> elements );

			
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