using System;

namespace SpiritIsland.Core {

	public enum From { Presence, SacredSite}

	public abstract class TargetSpaceAction : BaseAction {
		protected readonly Spirit self;
		public TargetSpaceAction(
			Spirit self,
			GameState gameState,
			int range,
			From from
		):base(gameState){
			this.self = self;
			switch(from){
				case From.SacredSite:
					engine.decisions.Push(new SelectSpaceRangeFromSacredSite(self,range,FilterSpace,SelectSpace));
					break;
				case From.Presence:
					engine.decisions.Push(new SelectSpaceRangeFromPresence(self,range,FilterSpace,SelectSpace));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(from));
			}

		}
		protected virtual bool FilterSpace(Space space)=>true;
		protected abstract void SelectSpace(Space space);
	}
}
