﻿using System;
using System.Collections.Generic;
using System.Linq;

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

		public string Summary { get {
			static int Order_CitiesTownsExplorers( Token invader )
				=> -(invader.FullHealth * 10 + invader.Health);
			return this.Invaders()
				.OrderBy( Order_CitiesTownsExplorers )
				.Select( invader => counts[invader] + invader.Summary )
				.Join( "," );
		} }

		#endregion

		public TokenBinding Blight => new TokenBinding( this, TokenType.Blight);
		public TokenBinding Defend => new TokenBinding( this, TokenType.Defend );

	}


}