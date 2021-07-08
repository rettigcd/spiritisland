﻿using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class SelectSpaceRangeFromPresence : IDecision {

		readonly Action<Space> onSelect;

		public SelectSpaceRangeFromPresence(
			Spirit spirit, 
			int range,
			Action<Space> onSelect
		):this(spirit,range,(s)=>true,onSelect){}

		public SelectSpaceRangeFromPresence(
			Spirit spirit, 
			int range,
			Func<Space,bool> spaceFilter,
			Action<Space> onSelect
		){
			this.onSelect = onSelect;

			this.Options = spirit.Presence
				.SelectMany(x => x.SpacesWithin(range))
				.Distinct()
				.Where(spaceFilter)
				.OrderBy(x=>x.Label)
				.ToArray();
		}

		public string Prompt => $"Select target space.";

		public IOption[] Options { get; }

		public void Select( IOption option ) {
			this.onSelect((Space)option);
		}

	}


}
