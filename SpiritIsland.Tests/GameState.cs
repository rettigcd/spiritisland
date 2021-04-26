
using System.Collections.Generic;

namespace SpiritIsland {

	public class GameState {
		public void AddDahanToSpace( Space space ){
			if(dahanCount.ContainsKey(space))
				dahanCount[space]++;
			else
				dahanCount.Add(space,1);
		}
		public int GetDahanOnSpace( Space space ){
			return dahanCount.ContainsKey(space) ? dahanCount[space] : 0;
		}

		public bool HasDahan( Space space ) => GetDahanOnSpace(space)>0;

		readonly Dictionary<Space,int> dahanCount = new Dictionary<Space, int>();
	}

}
