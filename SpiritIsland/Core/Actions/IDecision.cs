
namespace SpiritIsland {
	public interface IDecision {
		public string Prompt { get; }
		public IOption[] Options { get; }
		public void Select(IOption option);
	}

	public interface IDecisionPlus : IDecision {
		public bool AllowAutoSelect { get; }
	}

}
