﻿namespace SpiritIsland {
	public class ReclaimAll : GrowthAction {

		public ReclaimAll(Spirit spirit):base(spirit){}

		public override void Apply(){
			spirit.AvailableCards.AddRange( spirit.PlayedCards );
			spirit.PlayedCards.Clear();
		}

		public override bool IsResolved => true;

	}


}
