using System;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
	public class SpeedAttribute : Attribute {
		public Speed DisplaySpeed { get; }
		public SpeedAttribute(Speed speed ) { DisplaySpeed = speed; }

		public virtual bool IsActiveFor( Speed requestSpeed, CountDictionary<Element> _) {
			return DisplaySpeed.IsOneOf( requestSpeed, Speed.FastOrSlow );
		}
	}


}