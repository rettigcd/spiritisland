﻿namespace SpiritIsland {
	public class PowerCard {
		public PowerCard(string name, int cost, Speed speed, string elements ){
			this.Name = name;
			this.Cost = cost;
			this.Speed = speed;
			this.Elements = elements;
		}
		public string Name { get; }
		public int Cost { get; }
		public Speed Speed { get; }
		public string Elements { get; }
	}

}