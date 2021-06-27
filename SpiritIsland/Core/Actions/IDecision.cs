namespace SpiritIsland.Core {
	public interface IDecision {
		public string Prompt { get; }
		public IOption[] Options { get; }
		public void Select(IOption option,ActionEngine engine);
	}

}
