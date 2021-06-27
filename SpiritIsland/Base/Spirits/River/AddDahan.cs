using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class AddDahan : IAtomicAction {
		readonly Space target;
		readonly int count;
		public AddDahan(Space target, int count=1){
			this.target = target;
			this.count = count;
		}
		public void Apply( GameState gameState ) {
			gameState.AddDahan(target,count);
		}
	}

}



