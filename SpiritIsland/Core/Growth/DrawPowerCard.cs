namespace SpiritIsland.Core {

	public class DrawPowerCard : GrowthActionFactory {

		readonly int count;
		public DrawPowerCard(int count=1){ 
			this.count = count;
		}

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			spirit.PowerCardsToDraw += count;
			return new ResolvedAction();
		}

	}

}
