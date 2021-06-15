namespace SpiritIsland {

	public class InnateAction : IAction { // :INamedAction
		public InnatePower Innate { get; }

		public bool IsResolved => throw new System.NotImplementedException();

		public void Apply() {
			throw new System.NotImplementedException();
		}

		public IOption[] GetOptions() {
			throw new System.NotImplementedException();
		}

		public void Select(IOption option) {
			throw new System.NotImplementedException();
		}
	}

}



