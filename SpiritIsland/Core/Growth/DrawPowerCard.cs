﻿namespace SpiritIsland.Core {

	public class DrawPowerCard : GrowthAction {

		readonly int count;
		public DrawPowerCard(int count=1){ 
			this.count = count;
		}
		public override void Apply(){
			spirit.PowerCardsToDraw += count;
		}

		public override bool IsResolved => true; // !!! should change this to force drawing card

		public override IOption[] Options => throw new System.NotImplementedException();
	}

}
