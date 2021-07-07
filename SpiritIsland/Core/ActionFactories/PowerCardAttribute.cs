namespace SpiritIsland.Core {

	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class MinorCardAttribute : PowerCardAttribute {
		public MinorCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Minor,elements)
		{ }
	}

	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class MajorCardAttribute : PowerCardAttribute {
		public MajorCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Major,elements)
		{ }
	}


	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class SpiritCardAttribute : PowerCardAttribute {
		public SpiritCardAttribute(string name, int cost, Speed speed, params Element[] elements)
			:base(name,cost,speed,PowerType.Spirit,elements)
		{ }
	}

	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class PowerCardAttribute : System.Attribute {

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

	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class InnateOptionAttribute : System.Attribute {
		public InnateOptionAttribute(params Element[] elements){
			this.Elements = elements;
		}

		public Element[] Elements { get; }

	}


}