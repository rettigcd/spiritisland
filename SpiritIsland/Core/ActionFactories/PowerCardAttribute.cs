using System;

namespace SpiritIsland.Core {

	[AttributeUsage(AttributeTargets.Class)]
	public class MinorCardAttribute : PowerCardAttribute {
		public MinorCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Minor,elements)
		{ }
	}

	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class MajorCardAttribute : PowerCardAttribute {
		public MajorCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Major,elements)
		{ }
	}


	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
	public class SpiritCardAttribute : PowerCardAttribute {
		public SpiritCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Spirit,elements)
		{ }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class PowerCardAttribute : Attribute {

		protected PowerCardAttribute(string name, int cost, Speed speed, PowerType type, Element[] elements){
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

	public enum PowerType{ Minor, Major, Spirit }

	[AttributeUsage(AttributeTargets.Class)]
	public class InnateOptionAttribute : Attribute {
		public InnateOptionAttribute(params Element[] elements){
			this.Elements = elements;
		}

		public Element[] Elements { get; }

	}


}