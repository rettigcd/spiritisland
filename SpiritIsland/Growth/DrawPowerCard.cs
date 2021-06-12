namespace SpiritIsland {

	public class DrawPowerCard : GrowthAction {

		readonly int count;
		public DrawPowerCard(int count=1){ 
			this.count = count;
		}
		public override void Apply(){
			spirit.PowerCardsToDraw += count;
			spirit.MarkResolved( this );
		}

		public override bool IsResolved => true; // !!! should change this to force drawing card
	}


}
