using System;

namespace SpiritIsland.Core {
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class SpiritCardAttribute : BaseCardAttribute {
		public SpiritCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Spirit,elements)
		{ }
	}


}