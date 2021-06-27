namespace SpiritIsland.Core {

	public interface IAtomicAction {
		void Apply(GameState gameState);
	}

}
