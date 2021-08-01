namespace SpiritIsland.Core {

	public class DrawPowerCard : GrowthActionFactory {

		readonly int count;
		public DrawPowerCard(int count=1){ 
			this.count = count;
		}

		public override void Activate( ActionEngine engine ) {
			engine.Self.PowerCardsToDraw += count;
		}

	}

}
