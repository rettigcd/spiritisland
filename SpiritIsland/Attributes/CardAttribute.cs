using System;

namespace SpiritIsland {

	// Base Attribute type for Major / Minor / Spirit cards 
	[AttributeUsage(AttributeTargets.Method)]
	public class CardAttribute : Attribute {

		protected CardAttribute(string name, int cost, PowerType type, Element[] elements)
			:this(name,cost,type,new CountDictionary<Element>( elements ))
		{}

		protected CardAttribute(string name, int cost, PowerType type, CountDictionary<Element> elements){
			this.Name = name;
			this.Cost = cost;
			this.PowerType = type;
			this.Elements = elements;
		}

		public string Name { get; }
		public int Cost { get; }
		public CountDictionary<Element> Elements { get; }
		public PowerType PowerType { get; }

	}

}