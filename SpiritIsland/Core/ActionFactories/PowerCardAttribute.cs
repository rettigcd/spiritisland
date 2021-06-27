namespace SpiritIsland.Core {

	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class PowerCardAttribute : System.Attribute {

		public PowerCardAttribute(string name, int cost, Speed speed, params Element[] elements){
			this.Name = name;
			this.Cost = cost;
			this.Speed = speed;
			this.Elements = elements;
		}

		public string Name { get; }
		public int Cost { get; }
		public Speed Speed { get; }
		public Element[] Elements { get; }

	}

	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class InnateOptionAttribute : System.Attribute {
		public InnateOptionAttribute(params Element[] elements){
			this.Elements = elements;
		}

		public Element[] Elements { get; }

	}


}