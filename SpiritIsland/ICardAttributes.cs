namespace SpiritIsland.PowerCards {
	public interface ICardAttributes {
		string Name { get; }
		int Cost { get; }
		Speed Speed { get; }
		public Element[] Elements { get; }
	}

}