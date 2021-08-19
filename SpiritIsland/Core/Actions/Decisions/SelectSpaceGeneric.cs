using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class SelectSpaceGeneric : IDecision {

		readonly Action<Space> onSelect;

		public SelectSpaceGeneric(string prompt, IEnumerable<Space> options, Action<Space> onSelect, bool allowAutoSelect = true ){
			this.Prompt = prompt;
			this.onSelect = onSelect;
			this.AllowAutoSelect = allowAutoSelect;

			this.Options = options
				.OrderBy(x=>x.Label)
				.ToArray();
		}

		public bool AllowAutoSelect { get; }

		public string Prompt {get;}

		public IOption[] Options { get; }

		public void Select( IOption option ) {
			this.onSelect((Space)option);
		}

	}


}
