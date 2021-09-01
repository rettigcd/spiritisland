namespace SpiritIsland {
	public class TokenCounts {
		public void AddOneTo( Space space ) { counts[space]++; }
		public void RemoveOneFrom( Space space ) { counts[space]--; }
		public bool AreOn( Space s ) => counts[s] > 0;
		public int GetCount( Space s ) => counts[s];
		readonly CountDictionary<Space> counts = new CountDictionary<Space>();
	}

}
