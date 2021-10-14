using System.Collections.Generic;

namespace SpiritIsland {

	public class StrifedInvader : Token {

		static internal readonly StrifedInvaderGenerator Generator = new();

		public StrifedInvader(TokenGroup generic,Token[] seq,int health, int strifeCount)
			:base( generic, seq, health ) {
			StrifeCount = strifeCount;
		}
		public override string Summary => base.Summary + new string( '^', StrifeCount);
		public int StrifeCount { get; }

		#region class Generator

		public class StrifedInvaderGenerator {

			public Token WithStrife( Token orig, int strifeCount ) {
				if( strifeCount<0 )
					throw new System.ArgumentOutOfRangeException(nameof(strifeCount));
				if(strifeCount == 0)
					return orig.Generic[orig.Health];
				var seq = GetSequence( orig.Generic, strifeCount );

				return seq[orig.Health];
			}

			// Creates an invader health sequence and caches it.
			// uses Invader *Generic* as the key
			StrifedInvader[] GetSequence( TokenGroup generic, int strifeCount ) {
				string key = generic.Label+ strifeCount;
				if(sequenceCache.ContainsKey( key )) return sequenceCache[key];
				var seq = BuildHealthSequence( generic, strifeCount );
				sequenceCache.Add( key, seq );
				return seq;
			}

			static StrifedInvader[] BuildHealthSequence( TokenGroup generic, int strifeCount ) {
				int fullHealth = generic.Default.Health;
				var seq = new StrifedInvader[fullHealth + 1];
				for(int h = 0; h <= fullHealth; ++h)
					seq[h] = new StrifedInvader( generic, seq, h, strifeCount );
				return seq;
			}

			readonly Dictionary<string, StrifedInvader[]> sequenceCache = new();

		}

		#endregion

	}

}
