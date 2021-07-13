namespace SpiritIsland.Core {

	public class MoveInvader {

		public MoveInvader(Invader invader, Space from, Space to){
			this.invader = invader;
			this.from = from;
			this.to = to;
		}
		public void Apply( GameState gameState ) {
			gameState.Adjust(from,invader,-1);
			gameState.Adjust(to,invader,1);
		}
		readonly Invader invader;
		readonly Space from;
		readonly Space to;
	}

}
