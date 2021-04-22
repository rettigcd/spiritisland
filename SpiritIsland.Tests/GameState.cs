
using System.Collections.Generic;

namespace SpiritIsland {

	public class GameState {
		public void AddDahanToSpace( BoardSpace space ){
			if(dahanCount.ContainsKey(space))
				dahanCount[space]++;
			else
				dahanCount.Add(space,1);
		}
		public int GetDahanOnSpace( BoardSpace space ){
			return dahanCount.ContainsKey(space) ? dahanCount[space] : 0;
		}

		public bool HasDahan( BoardSpace space ) => GetDahanOnSpace(space)>0;

		readonly Dictionary<BoardSpace,int> dahanCount = new Dictionary<BoardSpace, int>();
	}

}
