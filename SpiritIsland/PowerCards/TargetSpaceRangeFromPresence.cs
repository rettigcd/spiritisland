using System;
using System.Linq;

namespace SpiritIsland.PowerCards {
	public class TargetSpaceRangeFromPresence : IDecision {

		readonly Action<Space,ActionEngine> onSelect;

		public TargetSpaceRangeFromPresence(
			Spirit spirit, 
			int range,
			Func<Space,bool> spaceFilter,
			Action<Space,ActionEngine> onSelect
		){
			this.onSelect = onSelect;

			this.Options = spirit.Presence
				.SelectMany(x => x.SpacesWithin(range))
				.Distinct()
				.Where(spaceFilter)
				.ToArray();
		}

		public IOption[] Options { get; }

		public void Select( IOption option, ActionEngine engine ) {
			this.onSelect((Space)option,engine);
		}

	}

}
