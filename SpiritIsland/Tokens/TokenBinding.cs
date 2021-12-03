namespace SpiritIsland {

	public class TokenBinding {

		readonly protected TokenCountDictionary counts;
		readonly Token token;

		public TokenBinding( TokenCountDictionary tokens, Token token ) {
			this.counts = tokens;
			this.token = token;
		}
		public bool Any => Count > 0;

		public virtual int Count => counts[token];

		public void Init(int count ) {
			counts[token] = count;
		}

		public void Add(int count) {
			counts[token] += count;
		}

		public virtual void Remove(int count) {
			counts[token] -= count;
		}

		public virtual void Destroy(int count) {
			counts[token] -= count;
			// !!! Should generate a destroy event
		}

		public static implicit operator int( TokenBinding b ) => b.Count;

	}


}
