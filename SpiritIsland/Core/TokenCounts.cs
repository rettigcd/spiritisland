namespace SpiritIsland {
	public class TokenCounts {
		public void AddOneTo( Space space ) { count[space]++; }
		public void RemoveOneFrom( Space space ) { count[space]--; }
		public bool AreOn( Space s ) => count[s] > 0;
		readonly CountDictionary<Space> count = new CountDictionary<Space>();
	}

}
