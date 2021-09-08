using System;
using System.Collections.Generic;

namespace SpiritIsland {

	public class TokenCountDictionary {

		#region constructor

		public TokenCountDictionary( Space space, CountDictionary<Token> counts ) {
			this.Space = space;
			this.counts = counts;
		}

		#endregion
		public Space Space { get; }

		public int this[Token specific] {
			get {
				ValidateIsAlive( specific );
				return counts[specific];
			}
			set {
				ValidateIsAlive( specific );
				counts[specific] = value; 
			}
		}

		public IEnumerable<Token> Keys => counts.Keys;

		#region private
		static void ValidateIsAlive( Token specific ) {
			if(specific.Health == 0) 
				throw new ArgumentException( "We don't store dead counts" );
		}

		readonly CountDictionary<Token> counts;

		#endregion

		public TokenBinding Blight => new TokenBinding(this,TokenType.Blight);
		public TokenBinding Defend => new TokenBinding( this, TokenType.Defend );

	}


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
