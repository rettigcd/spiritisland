using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]

	public class SpeedAttribute : Attribute {
		public Phase DisplaySpeed { get; }
		public SpeedAttribute(Phase speed ) { DisplaySpeed = speed; }

		public virtual Task<bool> IsActiveFor( Phase requestSpeed, Spirit _ ) {
			return Task.FromResult( DisplaySpeed.IsOneOf( requestSpeed, Phase.FastOrSlow ) );
		}
		public virtual bool CouldBeActiveFor( Phase requestSpeed, Spirit _ ) {
			return DisplaySpeed.IsOneOf( requestSpeed, Phase.FastOrSlow );
		}

	}


}