namespace SpiritIsland.Core {

	public class GatherDahanCtx{
		public GatherDahanCtx(Space target,GameState gameState){
			Target = target;
			DestinationCount = gameState.GetDahanOnSpace(target);
			foreach(var x in target.Neighbors)
				neighborCounts[x] = gameState.GetDahanOnSpace(x);
		}
		public readonly Space Target;
		public readonly CountDictionary<Space> neighborCounts = new CountDictionary<Space>();
		public int DestinationCount{
			get{  
				return _destinationCount; 
			}
			set{  
				_destinationCount = value; 
			}
		}
		int _destinationCount;
	}

}



