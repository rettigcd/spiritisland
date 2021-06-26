namespace SpiritIsland.PowerCards {
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class InnatePowerAttribute : System.Attribute , IPowerAttributes {
		public InnatePowerAttribute(string name, Speed speed){
			this.Name = name;
			this.Speed = speed;
		}

		public string Name { get; }
		public Speed Speed { get; }

	}

}