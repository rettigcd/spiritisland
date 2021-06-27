namespace SpiritIsland.Core {

	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class InnatePowerAttribute : System.Attribute {
		public InnatePowerAttribute(string name, Speed speed){
			this.Name = name;
			this.Speed = speed;
		}

		public string Name { get; }
		public Speed Speed { get; }

	}

}