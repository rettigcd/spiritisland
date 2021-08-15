using System;

namespace SpiritIsland {

	public class SelectSpirit : IDecision {

		readonly Action<Spirit> onSelect;

		public SelectSpirit( Spirit[] spirits, Action<Spirit> onSelect ){
			this.onSelect = onSelect;
			this.Options = spirits;
		}

		public string Prompt => $"Select spirit.";

		public IOption[] Options { get; }

		public void Select( IOption option ) {
			this.onSelect((Spirit)option);
		}

	}

}
