﻿namespace SpiritIsland.PowerCards {
	public class MoveInvader : IAtomicAction {
		public MoveInvader(Invader invader, Space from, Space to){
			this.invader = invader;
			this.from = from;
			this.to = to;
		}
		public void Apply( GameState gameState ) {
			gameState.Adjust(invader,from,-1);
			gameState.Adjust(invader,to,1);
		}
		readonly Invader invader;
		readonly Space from;
		readonly Space to;
	}

}
