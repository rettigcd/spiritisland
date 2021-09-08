namespace SpiritIsland {
	public class TokenBinding {

		TokenCountDictionary counts;
		Token token;
		public TokenBinding( TokenCountDictionary tokens, Token token ) {
			this.counts = tokens;
			this.token = token;
		}
		public bool Any => Count > 0;
		public int Count {
			get => counts[token];
			set => counts[token] = value;
		}

		public static implicit operator int( TokenBinding b ) => b.Count;

		//public static TokenBinding operator --( TokenBinding s ) { --s.Count; return s; }
		//public static TokenBinding operator ++( TokenBinding s ) { ++s.Count; return s; }

	}


}
