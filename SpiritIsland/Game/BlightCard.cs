using System;

namespace SpiritIsland {

	public class BlightCard {

		static public readonly BlightCard MemoryFadesToDust = new BlightCard{
			Title = "Memory Fades to Dust",
			healthyBlightPerPlayer = 2,
			blightedBlightPerPlayer = 4,
			// At the start of each invader phase each spirit forgets a power or destorys 1 of their presence
			BlightAction = (gs) => { }, // !!!
		};

		static public readonly BlightCard DownwardSpiral = new BlightCard{
			Title = "Downward Spiral",
			healthyBlightPerPlayer = 2,
			blightedBlightPerPlayer = 5,
			// at the start of each invader phase each spirit destorys 1 of their presence
			BlightAction = (gs) => { }, // !!!
		};

		protected int healthyBlightPerPlayer;
		protected int blightedBlightPerPlayer;

		public string Title;
		public int StartingBlight(GameState gameState) => gameState.Spirits.Length * healthyBlightPerPlayer;
		public int AdditionalBlight(GameState gameState) => gameState.Spirits.Length * blightedBlightPerPlayer;
		public Action<GameState> BlightAction;
	}


}
