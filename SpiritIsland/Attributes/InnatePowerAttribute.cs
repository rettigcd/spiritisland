using System;

namespace SpiritIsland {

	[AttributeUsage(AttributeTargets.Class)]
	public class InnatePowerAttribute : System.Attribute {
		public InnatePowerAttribute(string name){
			this.Name = name;
		}

		public string Name { get; }

	}

}