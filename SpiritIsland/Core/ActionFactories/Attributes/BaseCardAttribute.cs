using System;

namespace SpiritIsland.Core {

	// Base Attribute type for Major / Minor / Spirit cards 
	[AttributeUsage(AttributeTargets.Method)]
	public class BaseCardAttribute : Attribute {

		protected BaseCardAttribute(string name, int cost, Speed speed, PowerType type, Element[] elements){
			this.Name = name;
			this.Cost = cost;
			this.Speed = speed;
			this.PowerType = type;
			this.Elements = elements;
		}

		public string Name { get; }
		public int Cost { get; }
		public Speed Speed { get; }
		public Element[] Elements { get; }
		public PowerType PowerType { get; }

	}


}