using System;

namespace SpiritIsland {

	public class TokenBinding : IDefendTokenBindings {

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
			counts[token] += count;					// !!! Token added event?
		}

		public virtual void Remove(int count) {
			counts[token] -= count;					// !!! token removed event?
		}

		public virtual void Destroy(int count) {
			counts[token] -= count;
													// !!! Should generate a destroy event
		}

		public static implicit operator int( TokenBinding b ) => b.Count;

	}

	public class DefendTokenBinding : IDefendTokenBindings {
		readonly Space space;
		readonly CountDictionary<Token> tokens;
		readonly Func<Space, int> virtualDefend;
		public DefendTokenBinding( Space space, CountDictionary<Token> tokens, Func<Space, int> virtualDefend ) {
			this.space = space;
			this.tokens = tokens;
			this.virtualDefend = virtualDefend;
		}

		public int Count => tokens[TokenType.Defend]
			+ virtualDefend(space);

		public void Add( int count ) {
			tokens[TokenType.Defend] += count;  // !!! this should trigger token-added event
		}

	}

}
