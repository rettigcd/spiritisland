using System;

namespace SpiritIsland {

	// Base Attribute type for Major / Minor / Spirit cards 
	[AttributeUsage(AttributeTargets.Method)]
	public class CardAttribute : Attribute {

		protected CardAttribute(string name, int cost, PowerType type, Element[] elements)
			:this(name,cost,type,new ElementCounts( elements ))
		{}

		protected CardAttribute(string name, int cost, PowerType type, ElementCounts elements){
			this.Name = name;
			this.Cost = cost;
			this.PowerType = type;
			this.Elements = elements;
		}

		public string Name { get; }
		public int Cost { get; }
		public ElementCounts Elements { get; }
		public PowerType PowerType { get; }

	}

}