namespace SpiritIsland.PowerCards {
	public interface IDecision {
		public IOption[] Options { get; }
		public void Select(IOption option,ActionEngine engine);
	}

}
