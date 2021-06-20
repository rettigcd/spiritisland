namespace SpiritIsland.PowerCards {
	public class GatherDahanCtx{
		public GatherDahanCtx(Space target,GameState gameState){
			Target = target;
			destinationCount = gameState.GetDahanOnSpace(target);
			foreach(var x in target.SpacesExactly(1))
				neighborCounts[x] = gameState.GetDahanOnSpace(x);
		}
		public readonly Space Target;
		public readonly CountDictionary<Space> neighborCounts = new CountDictionary<Space>();
		public int destinationCount;
	}

}



