using System;

namespace SpiritIsland {
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class MajorCardAttribute : BaseCardAttribute {
		public MajorCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Major,elements)
		{ }
	}


}