using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public enum From { Presence, SacredSite}

	public abstract class TargetSpaceAction : BaseAction {
		protected readonly Spirit self;
		public TargetSpaceAction(
			Spirit self,
			GameState gameState,
			int range,
			From from
		):base(self,gameState){
			this.self = self;

			IEnumerable<Space> source = from switch {
				From.Presence => self.Presence,
				From.SacredSite => self.SacredSites,
				_ => throw new ArgumentOutOfRangeException(nameof(from))
			};
			decisions.Push(new SelectSpaceGeneric(
				"Select target."
				,source.Range(range).Where(FilterSpace)
				,SelectSpace // ! overriden by drived class, might not be initialized yet
			));
		}
		protected virtual bool FilterSpace(Space space)=>true;
		protected abstract void SelectSpace(Space space);
	}
}
