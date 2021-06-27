namespace SpiritIsland.Core {

	public class DrawPowerCard : GrowthAction {

		readonly int count;
		public DrawPowerCard(int count=1){ 
			this.count = count;
		}

		public override IAction Bind( Spirit spirit, GameState gameState ) {
			return new ResolvedAction(()=>spirit.PowerCardsToDraw += count);
		}

	}

}
