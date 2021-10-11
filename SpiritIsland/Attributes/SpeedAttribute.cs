using System;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Method)]
	public class SpeedAttribute : Attribute {
		public Speed Speed { get; }
		public SpeedAttribute(Speed speed ) { Speed = speed; }
	}

	public class FastAttribute : SpeedAttribute {
		public FastAttribute() : base( Speed.Fast ) { }
	}

	public class SlowAttribute : SpeedAttribute {
		public SlowAttribute() : base( Speed.Slow ) { }
	}


}