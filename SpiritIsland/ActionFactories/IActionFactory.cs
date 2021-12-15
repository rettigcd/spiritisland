using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public record ActionTaken ( IActionFactory ActionFactory, object Context );

	public interface IActionFactory : IOption {

		Task ActivateAsync( SelfCtx ctx ); // returns Target if any

		bool CouldActivateDuring( Phase speed, Spirit spirit );

		string Name { get; }
			
	}

	public interface IFlexibleSpeedActionFactory : IActionFactory {
		Phase DisplaySpeed { get; }
		/// <summary> When set, overrides the speed attribute for everything except Display Speed </summary>
		ISpeedBehavior OverrideSpeedBehavior { get; set; }
	}

	public interface IRecordLastTarget {
		public object LastTarget { get; }
	}

}