using System;

namespace SpiritIsland.Core {

	public class TargetSpirit : IDecision {

		readonly Action<Spirit> onSelect;

		public TargetSpirit( Spirit[] spirits, Action<Spirit> onSelect ){
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
