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

	}


}
