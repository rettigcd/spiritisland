namespace SpiritIsland {
	public class DrawPowerCard : GrowthAction {
		readonly int count;
		public DrawPowerCard(int count=1){ this.count = count; }
		public override void Apply(Spirit ps){
			ps.PowerCardsToDraw += count;
		}
	}


}
