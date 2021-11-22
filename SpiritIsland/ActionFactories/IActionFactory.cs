using System.Threading.Tasks;

namespace SpiritIsland {

	public interface IActionFactory : IOption {

		Task ActivateAsync( SpiritGameStateCtx ctx );

		bool CouldActivateDuring( Phase speed, Spirit spirit );

		string Name { get; }
			
	}

	public interface IFlexibleSpeedActionFactory : IActionFactory {
		Phase DisplaySpeed { get; }
		/// <summary> When set, overrides the speed attribute for everything except Display Speed </summary>
		ISpeedBehavior OverrideSpeedBehavior { get; set; }
	}

}