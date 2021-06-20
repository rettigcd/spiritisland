namespace SpiritIsland.PowerCards {
	public class MoveDahan : IAtomicAction {
		readonly Space from;
		readonly Space to;
		public MoveDahan(Space from, Space to){
			this.from = from;
			this.to = to;
		}
		public void Apply( GameState gameState ) {
			gameState.AddDahan(from,-1);
			gameState.AddDahan(to,1);
		}
	}

}



