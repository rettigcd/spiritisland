using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(LightningsBoon.Name,1,Speed.Fast,Element.Fire,Element.Air)]
	public class LightningsBoon : TargetSpiritAction {
		public const string Name = "Lightning's Boon";

		public LightningsBoon(Spirit spirit,GameState gs):base(spirit,gs){}

		protected override void SelectSpirit( Spirit spirit ) {
			engine.decisions.Push(new SelectActionsToMakeFast(engine,spirit,2));
		}

		// any spirt
		// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.

	}

}
