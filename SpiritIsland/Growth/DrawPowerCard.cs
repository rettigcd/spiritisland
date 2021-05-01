﻿namespace SpiritIsland {
	public class DrawPowerCard : GrowthAction {
		readonly int count;
		public DrawPowerCard(Spirit spirit, int count=1):base(spirit){ 
			this.count = count;
		}
		public override void Apply(){
			spirit.PowerCardsToDraw += count;
		}
	}


}