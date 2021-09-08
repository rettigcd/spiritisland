namespace SpiritIsland {
	/// <summary>
	/// Provides a single point to process a stream of decisions / choices
	/// </summary>
	public interface IDecisionStream {

		public IDecision GetCurrent();

		public void Choose( IOption option );

		public bool IsResolved { get; }
	}

}
