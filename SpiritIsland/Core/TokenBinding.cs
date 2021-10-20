using System.Collections.Generic;

namespace SpiritIsland {

	public class TokenBinding {

		readonly TokenCountDictionary counts;
		readonly Token token;
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

	public class TokenGroupBinding {

		readonly TokenCountDictionary counts;
		readonly TokenGroup tokenGroup;
		public TokenGroupBinding( TokenCountDictionary tokens, TokenGroup tokenGroup ) {
			this.counts = tokens;
			this.tokenGroup = tokenGroup;
		}

		public IEnumerable<Token> Keys => counts.OfType(tokenGroup);

		public bool Any => Count > 0;
		public int Count => counts.Sum(tokenGroup);

		public int this[int index] {
			get => counts[tokenGroup[index]];
			set => counts[tokenGroup[index]] = value;
		}

		public void Add(int count) => counts[tokenGroup.Default] += count;

		public static implicit operator int( TokenGroupBinding b ) => b.Count;

	}


}
