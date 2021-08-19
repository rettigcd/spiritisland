
namespace SpiritIsland {
	public interface IDecision {
		public string Prompt { get; }
		public IOption[] Options { get; }
		public void Select(IOption option);
		public bool AllowAutoSelect { get; }
	}

}
