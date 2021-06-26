using System;
using System.Linq;

namespace SpiritIsland.PowerCards {
	public class TargetSpaceRangeFromSacredSite : IDecision {

		readonly Action<Space,ActionEngine> onSelect;

		public TargetSpaceRangeFromSacredSite(
			Spirit spirit, 
			int range,
			Func<Space,bool> spaceFilter,
			Action<Space,ActionEngine> onSelect
		){
			this.onSelect = onSelect;

			this.Options = spirit.SacredSites
				.SelectMany(x => x.SpacesWithin(range))
				.Distinct()
				.Where(spaceFilter)
				.OrderBy(x=>x.Label)
				.ToArray();
		}

		public string Prompt => $"Select target space.";

		public IOption[] Options { get; }

		public void Select( IOption option, ActionEngine engine ) {
			this.onSelect((Space)option,engine);
		}

	}


}
