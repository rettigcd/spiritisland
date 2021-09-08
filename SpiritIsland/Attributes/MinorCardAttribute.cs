using System;

namespace SpiritIsland {
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class MinorCardAttribute : CardAttribute {
		public MinorCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Minor,elements)
		{ }
		public MinorCardAttribute( string name, int cost, Speed speed, string elementString )
			: base( name, cost, speed, PowerType.Minor, ElementList.Parse(elementString) ) { }
	}


}