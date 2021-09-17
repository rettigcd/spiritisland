using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IActionFactory : IOption {

		Task ActivateAsync(Spirit spirit, GameState gameState);
		Speed Speed { get; }
		Speed DefaultSpeed { get; }

		public SpeedOverride OverrideSpeed { get; set; }

		string Name { get; }
//		IActionFactory Original { get; }

		// Used by Innates to set Triggered / Not Triggered
		// Used by certain innates and Cards to determine speed
		void UpdateFromSpiritState( CountDictionary<Element> elements );

			
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