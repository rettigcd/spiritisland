namespace SpiritIsland.PowerCards {

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

}