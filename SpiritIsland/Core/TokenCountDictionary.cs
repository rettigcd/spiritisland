using System;
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

		public string InvaderSummary { get {
			static int Order_CitiesTownsExplorers( Token invader )
				=> -(invader.FullHealth * 10 + invader.Health);
			return this.Invaders()
				.OrderBy( Order_CitiesTownsExplorers )
				.Select( invader => counts[invader] + invader.Summary )
				.Join( "," );
		} }

		public string Summary { get {
			return this.Keys
				.OrderBy( k=>k.Summary )
				.Select( invader => counts[invader] + invader.Summary )
				.Join( "," );
		} }

		public override string ToString() => Space.Label + ":" + Summary;

		public TokenBinding Blight => new TokenBinding( this, TokenType.Blight);
		public TokenBinding Defend => new TokenBinding( this, TokenType.Defend );
		public TokenBinding Beasts => new ( this, TokenType.Beast );
		public TokenBinding Disease => new ( this, TokenType.Disease );
		public TokenBinding Wilds => new ( this, TokenType.Wilds );
		public TokenBinding Badlands => new ( this, TokenType.Badlands );
		public TokenGroupBinding Dahan => new ( this, TokenType.Dahan );

		#region private

		static void ValidateIsAlive( Token specific ) {
			if(specific.Health == 0) 
				throw new ArgumentException( "We don't store dead counts" );
		}

		readonly CountDictionary<Token> counts;

		#endregion

		public void Adjust( Token specific, int delta ) {
			if(specific.Health == 0) throw new System.ArgumentException( "Don't try to track dead tokens." );
			counts[specific] += delta;
		}

		public void AddStrifeTo( Token invader, int count = 1 ) {

			// Remove old type from 
			if(this[invader]<count)
				throw new ArgumentOutOfRangeException($"collection does not contain {count} {invader.Summary}");
			this[invader] -= count;

			// Add new strifed
			int curStrifeCount = invader is StrifedInvader si ? si.StrifeCount : 0;
			var strifed = StrifedInvader.Generator.WithStrife(invader, curStrifeCount +1 );

			this[strifed] += count;
		}

		public Token RemoveStrife( StrifedInvader orig, int tokenCount ) {
			Token lessStrifed = orig.AddStrife( -1 );
			this[lessStrifed] += tokenCount;
			this[orig] -= tokenCount;
			return lessStrifed;
		}


	}


}
