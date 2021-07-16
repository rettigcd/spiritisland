using System;

namespace SpiritIsland.Core {

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class InnatePowerAttribute : System.Attribute {
		public InnatePowerAttribute(string name, Speed speed){
			this.Name = name;
			this.Speed = speed;
		}

		public string Name { get; }
		public Speed Speed { get; }

	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class PowerLevelAttribute : System.Attribute {

		public PowerLevelAttribute(int level,params Element[] elements){
			this.Level = level;
			this.Elements = elements;
		}

		public int Level { get; }
		public Element[] Elements { get; }

	}


}